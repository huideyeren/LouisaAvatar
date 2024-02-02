#ifndef VIRTUALLENS2_SYSTEM_COC_WRITER_CGINC
#define VIRTUALLENS2_SYSTEM_COC_WRITER_CGINC

#include "UnityCG.cginc"
#include "../Common/Constants.cginc"
#include "../Common/TargetDetector.cginc"
#include "../Common/StateTexture.cginc"
#include "../Common/FieldOfView.cginc"

//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

float _FieldOfView;       // Raw field of view [deg]
float _LogFNumber;        // log(F)
float _LogFocusDistance;  // log(distance to focus plane [m])
float _BlurringThresh;    // Disable DoF simulation when _LogFNumber >= _BlurringThresh
float _FocusingThresh;    // Use manual focusing when _LogFocusDistance > _FocusingThresh

sampler2D_float _CameraDepthTexture;  // Current camera's depth texture


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
	float2 params : TEXCOORD1;  // (max background CoC, distance)
};


//---------------------------------------------------------------------------
// Vertex shader
//---------------------------------------------------------------------------

bool isBlurringEnabled(){
	return _LogFNumber < _BlurringThresh;
}

v2f vertex(appdata v){
	v2f o;

	if(!isVirtualLensCaptureCamera()){ // || !isBlurringEnabled()){
		o.vertex = 0;
		o.uv     = 0;
		o.params = 0;
		return o;
	}

	const float4 focus_point = float4(getFocusPointState(), 0, 0);

	const float f = computeFocalLength(_FieldOfView);
	const float a = f / exp(_LogFNumber);
	const float p = (_LogFocusDistance > _FocusingThresh)
		? exp(_LogFocusDistance)
		: LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, focus_point));

	// Maximum CoC in pixels when screen size equals to (1920, 1080)
	const float max_coc = ((a * f) / (p - f)) * (1920.0 / SENSOR_SIZE) * 0.5;

	const float x = (v.uv.x * 2 - 1);
	const float y = (v.uv.y * 2 - 1) * _ProjectionParams.x;
	o.vertex = float4(x, y, 0, 1);
	o.uv     = v.uv;
	o.params = float2(max_coc, p);
	return o;
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

float4 fragment(v2f i) : SV_Target {
	if(_BlurringThresh == _LogFNumber){
		const float z = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv));
		return float4(0, 0, 0, z);
	}

	const float max_coc = i.params.x;
	const float p = i.params.y;
	const float z = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv));
	const float coc = (1 - p / z) * max_coc;
	return float4(0, 0, 0, coc);
}

#endif
