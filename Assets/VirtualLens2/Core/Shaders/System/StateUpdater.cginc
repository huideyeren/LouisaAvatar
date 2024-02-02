#ifndef VIRTURLLENS2_SYSTEM_STATE_UPDATER_CGINC
#define VIRTURLLENS2_SYSTEM_STATE_UPDATER_CGINC

#include "UnityCG.cginc"
#include "../Common/Constants.cginc"
#include "../Common/Depth.cginc"
#include "../Common/TargetDetector.cginc"
#include "../Common/CameraPoseTexture.cginc"
#include "../Common/MultiSamplingHelper.cginc"
#include "../AutoFocus/AutoFocusTexture.cginc"


//---------------------------------------------------------------------------
// Parameters
//---------------------------------------------------------------------------

float _AFMode;            // Auto focus mode
float _AFSpeed;           // Auto focus speed
float _Near;              // Distance to near plane [m]
float _Far;               // Distance to far plane [m]
float _FieldOfView;       // Raw field of view [deg]
float _LogFocusDistance;  // log(distance to focus plane [m])
float _FocusingThresh;    // Use manual focusing when _LogFocusDistance > _FocusingThresh
float _FocusLock;         // Lock focus distance

TEXTURE2DMS<float> _DepthTex;        // Depth texture
Texture2D<float4>  _TouchScreenTex;  // Texture captured by touch detector
Texture2D<float4>  _SelfieFocusTex;  // Texture captured by selfie eye detector
Texture2D<float4>  _GrabTexture;     // Previous state


// AF Modes
static const int AF_MODE_POINT  = 0;
static const int AF_MODE_FACE   = 1;
static const int AF_MODE_SELFIE = 2;


//---------------------------------------------------------------------------
// Struct definitions
//---------------------------------------------------------------------------

struct appdata {
	float4 vertex : POSITION;
	float2 uv     : TEXCOORD0;
};

struct v2f {
	float4 vertex : SV_POSITION;
	float2 uv     : TEXCOORD0;
};


//---------------------------------------------------------------------------
// Vertex shader
//---------------------------------------------------------------------------

v2f vertex(appdata v){
	v2f o;

	if(!isVirtualLensStateUpdaterCamera()){
		o.vertex = 0;
		o.uv     = 0;
		return o;
	}

	const float x = (v.uv.x * 2 - 1);
	const float y = (v.uv.y * 2 - 1) * _ProjectionParams.x;
	o.vertex = float4(x, y, 0, 1);
	o.uv     = v.uv;
	return o;
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

// Touching point (w/ touch screen extensions)
float4 computeTouchingPoint(){
	const float4 raw = _TouchScreenTex[int2(0, 0)];
	if(raw.w != 0.0625){
		// Not touching: keep old values
		const float4 old = _GrabTexture[int2(0, 0)];
		if(old.x < 0.5){
			return float4(1, 0.5, 0.5, 1);  // Initialize to center
		}else{ 
			return old;
		}
	}else{
		const float2 scale  = INV_SCREEN_ENLARGEMENT;
		const float2 offset = (1.0 - scale) * 0.5;
		return float4(1, raw.xy * scale + offset, 1);
	}
}

// Focus point in world space
float4 computeFocusPoint(){
	const int mode = (int)round(_AFMode);
	if(_LogFocusDistance == _FocusingThresh && mode == AF_MODE_FACE){
		const float4x4 v2w = getInversedViewMatrix();
		const float4 ev = getEyeFocusingPoint();
		if(ev.w > 0){ return float4(mul(v2w, ev).xyz, 0); }
		const float4 fv = getFaceFocusingPoint();
		if(fv.w > 0){ return float4(mul(v2w, fv).xyz, 0); }
		const float4 pv = getPixelFocusingPoint();
		if(pv.w > 0){ return float4(mul(v2w, pv).xyz, 0); }
		return _GrabTexture[int2(1, 0)];
	}else{
		return _GrabTexture[int2(1, 0)];
	}
}

// Bounding box from AF engine
float4 computeBoundaryBox(){
	const int mode = (int)round(_AFMode);
	if(_LogFocusDistance == _FocusingThresh && mode == AF_MODE_FACE){
		const float4 eye = getEyeBoundaryBox();
		if(eye.z * eye.w > 0){ return eye; }
		const float4 face = getFaceBoundaryBox();
		if(face.z * face.w > 0){ return face; }
		const float4 pixel = getPixelFocusingPoint();
		if(pixel.w > 0){
			const float z = pixel.z;
			const float y_scale = tan((PI - _FieldOfView * DEG2RAD) * 0.5);
			const float x_scale = y_scale / ASPECT;
			const float x = pixel.x * x_scale / z;
			const float y = pixel.y * y_scale / z;
			const float u = (x + 1.0) * 0.5;
			const float v = (y + 1.0) * 0.5;
			const float w = 0.0125;
			const float h = w * ASPECT;
			return float4(u, v, w, h);
		}
		return 0;
	}else{
		return 0;
	}
}

// Focus distance
float computeTargetFocusDistance(float old){
	const int mode = (int)round(_AFMode);
	if((int)round(_FocusLock)){
		return old;
	}else if(mode == AF_MODE_POINT){
		// Point AF
		uint width = 0, height = 0, num_samples = 0;
		TEXTURE2DMS_GET_DIMENSIONS(_DepthTex, width, height, num_samples);
		const float2 uv = computeTouchingPoint().yz;
		const int2 pos = (int2)floor(uv * float2(width, height));
		return linearizeDepth(_DepthTex.Load(TEXTURE2DMS_COORD(pos), 0), _Near, _Far);
	}else if(mode == AF_MODE_FACE){
		// Face detection
		const float4 ev = getEyeFocusingPoint();
		if(ev.w > 0){ return ev.z; }
		const float4 fv = getFaceFocusingPoint();
		if(fv.w > 0){ return fv.z; }
		const float4 pv = getPixelFocusingPoint();
		if(pv.w > 0){ return pv.z; }
		return old;
	}else if(mode == AF_MODE_SELFIE){
		// Selfie AF
		const float4 l = _SelfieFocusTex[int2(0, 0)];
		const float4 r = _SelfieFocusTex[int2(1, 0)];
		float z = 1.#INF;
		if(l.w == 0.0625){ z = min(z, l.x); }
		if(r.w == 0.0625){ z = min(z, r.x); }
		if(z == 1.#INF){ return old; }
		return z;
	}else{
		return old;
	}
}

float computeFocusDistance(){
	if(_LogFocusDistance > _FocusingThresh){
		// Manual focusing
		return exp(_LogFocusDistance);
	}
	const float linear_current = _GrabTexture[int2(3, 0)].x;
	const float linear_target  = computeTargetFocusDistance(linear_current);
	if(linear_current <= 0){ return linear_target; }
	const float log_current = log(linear_current);
	const float log_target  = log(linear_target);
	const float limit = _AFSpeed * unity_DeltaTime.x;
	if(abs(log_current - log_target) <= limit){ return linear_target; }
	if(log_target < log_current){
		return exp(log_current - limit);
	}else{
		return exp(log_current + limit);
	}
}

float2 computeDetectionStamps(){
	const int mode = (int)round(_AFMode);
	float2 old = _GrabTexture[int2(3, 0)].yz;
	if(_LogFocusDistance == _FocusingThresh && mode == AF_MODE_FACE){
		const float4 ev = getEyeFocusingPoint();
		if(ev.w > 0){ return float2(_Time.y, _Time.y); }
		const float4 fv = getFaceFocusingPoint();
		if(fv.w > 0){ return float2(_Time.y, _Time.y); }
		const float4 pv = getPixelFocusingPoint();
		if(pv.w > 0){ return float2(old.x, _Time.y); }
		return old;
	}else{
		return old;
	}
}

// Entry point
float4 fragment(v2f i) : SV_Target {
	const float2 coord = floor(i.uv * _ScreenParams.xy);
	if(coord.x == 0){
		return computeTouchingPoint();
	}else if(coord.x == 1){
		return computeFocusPoint();
	}else if(coord.x == 2){
		return computeBoundaryBox();
	}else if(coord.x == 3){
		return float4(
			computeFocusDistance(),
			computeDetectionStamps(),
			0.0);
	}
	return 0;
}

#endif
