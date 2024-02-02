#ifndef VIRTUALLENS2_SYSTEM_DISPLAY_RENDERER_CGINC
#define VIRTUALLENS2_SYSTEM_DISPLAY_RENDERER_CGINC

#include "UnityCG.cginc"
#include "../Common/Constants.cginc"
#include "../Common/TargetDetector.cginc"
#include "../Common/StateTexture.cginc"
#include "../Common/FieldOfView.cginc"
#include "../Common/MultiSamplingHelper.cginc"
#include "../AutoFocus/AutoFocusTexture.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

Texture2D<float4>  _MainTex;        // Rendered texture
TEXTURE2DMS<float> _DepthTex;       // Depth texture
Texture2D<float4>  _ComponentTex;   // Components texture
Texture2D<float4>  _CameraPoseTex;  // Camera pose / View matrix
Texture2D<float4>  _CustomGrid0Tex; // Custom grid 0
Texture2D<float4>  _CustomGrid1Tex; // Custom grid 1
Texture2D<float4>  _CustomGrid2Tex; // Custom grid 2
Texture2D<float4>  _CustomGrid3Tex; // Custom grid 3

float _GridType;     // Display grids
float _ShowInfo;     // Display parameters
float _ShowLevel;    // Display level meters
float _PeakingMode;  // Display peaking
float _CursorType;   // Display cursor

float _Near;              // Distance to near plane
float _Far;               // Distance to far plane
float _FieldOfView;       // Field of view [deg]
float _LogFNumber;        // log(F)
float _LogFocusDistance;  // log(distance to focus plane [m])
float _BlurringThresh;    // Disable DOF simulation when _LogFNumber >= _BlurringThresh
float _FocusingThresh;    // Use manual focusing when _LogFocusDistance > _FocusingThresh

SamplerState point_clamp_sampler;
SamplerState linear_clamp_sampler;


//---------------------------------------------------------------------------
// Enumerates and constants
//---------------------------------------------------------------------------

static const int GRID_NONE         = 0;
static const int GRID_3X3          = 1;
static const int GRID_3X3_DIAGONAL = 2;
static const int GRID_6X4          = 3;
static const int GRID_CUSTOM0      = 4;
static const int GRID_CUSTOM1      = 5;
static const int GRID_CUSTOM2      = 6;
static const int GRID_CUSTOM3      = 7;

static const int CURSOR_NONE  = 0;
static const int CURSOR_POINT = 1;

static const int DISPLAY_WIDTH  = 1280;
static const int DISPLAY_HEIGHT =  720;


//---------------------------------------------------------------------------
// Structure definitions
//---------------------------------------------------------------------------

struct appdata {
	float4 vertex : POSITION;
	float2 uv     : TEXCOORD0;
};

struct v2f {
	float4 vertex : SV_POSITION;
	float2 uv     : TEXCOORD0;
	float2 params : TEXCOORD1;  // (Packed focal, Packed F)
	float2 angles : TEXCOORD2;  // (Roll, Pitch)
	float2 focus  : TEXCOORD3;  // In-focus depth min and max
};


//---------------------------------------------------------------------------
// Vertex shader
//---------------------------------------------------------------------------

float2 extractRollPitch(){
	const float m01 = _CameraPoseTex[int2(0, 0)].y;
	const float m11 = _CameraPoseTex[int2(1, 0)].y;
	const float m21 = _CameraPoseTex[int2(2, 0)].y;
	return float2(
		atan2(m01, m11),  // Roll
		asin(m21));       // Pitch
}

float2 computeFocusRange(float p, float max_coc){
	if(_LogFNumber >= _BlurringThresh){ return float2(0.0, 1.0); }
	const float z = 1.0 / _Far - 1.0 / _Near;
	const float w = 1.0 / _Near;
	const float nume = max_coc * (p * z + p * w - 1.0);
	const float deno = max_coc * p * z;
	return float2(nume + 0.71, nume - 0.71) / deno;
}

v2f vertex(appdata v){
	v2f o;

	if(!isVirtualLensCustomComputeCamera(DISPLAY_WIDTH, DISPLAY_HEIGHT)){
		o.vertex = 0;
		o.uv     = 0;
		o.params = 0;
		o.angles = 0;
		o.focus  = 0;
		return o;
	}

	const float f = computeFocalLength(_FieldOfView);
	const float a = f / exp(_LogFNumber);
	const float p = getFocusDistance();
	// Max CoC radius in full resolution
	const float max_coc = ((a * f) / (p - f)) * (1920.0 / SENSOR_SIZE) * 0.5;

	const float x = (v.uv.x * 2 - 1);
	const float y = (v.uv.y * 2 - 1) * _ProjectionParams.x;
	o.vertex = float4(x, y, 0, 1);
	o.uv     = v.uv;
	o.params = 0;
	o.angles = extractRollPitch();
	o.focus  = computeFocusRange(p, max_coc);
	return o;
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

// Utility: alpha blending
float4 blend(float4 dst, float4 src){
	return float4(dst.rgb * (1.0 - src.a) + src.rgb * src.a, 1.0);
}

// Utility: point-to-line distance
float distance(float2 p0, float2 p1, float2 p2){
	const float nume = abs((p2.x - p1.x) * (p1.y - p0.y) - (p1.x - p0.x) * (p2.y - p1.y));
	const float deno = sqrt((p2.x - p1.x) * (p2.x - p1.x) + (p2.y - p1.y) * (p2.y - p1.y));
	return nume / deno;
}

// Captured image
float4 drawCapturedImage(float2 uv){
	const float2 scale = INV_SCREEN_ENLARGEMENT;
	const float2 t = uv * scale + (1.0 - scale) * 0.5;
	return _MainTex.Sample(linear_clamp_sampler, t);
}

// Focus peaking
float loadDepth(float2 uv){
	uint width = 0, height = 0, num_samples = 0;
	TEXTURE2DMS_GET_DIMENSIONS(_DepthTex, width, height, num_samples);
	const int2 xy = (int2)(uv * float2(width, height));
	return _DepthTex.Load(TEXTURE2DMS_COORD(xy), 0);
}

float4 drawFocusPeaking(float4 dst, float2 uv, float2 range){
	const float2 scale  = INV_SCREEN_ENLARGEMENT;
	const float2 offset = (1.0 - scale) * 0.5;
	const float d = loadDepth(uv * scale + offset);
	const int x = floor(uv.x * DISPLAY_WIDTH);
	const int y = floor(uv.y * DISPLAY_HEIGHT);
	const int parity = (x - y) & 15;
	if(parity == 0 && range.x <= d && d <= range.y){
		return float4(0, 1, 0, 1);
	}else{
		return dst;
	}
}

// Auto focusing marker
float4 drawAutoFocusMarker(float4 dst, float2 uv){
	const float2 scale  = INV_SCREEN_ENLARGEMENT;
	const float2 offset = (1.0 - scale) * 0.5;
	const float4 box = getDetectionBoundary();
	uv = uv * scale + offset;
	if(box.z * box.w > 0){
		const float2 p0 = box.xy - 0.5 * box.zw;
		const float2 p1 = box.xy + 0.5 * box.zw;
		const int x  = (int)floor(uv.x * DISPLAY_WIDTH);
		const int y  = (int)floor(uv.y * DISPLAY_HEIGHT);
		const int x0 = (int)floor(p0.x * DISPLAY_WIDTH);
		const int y0 = (int)floor(p0.y * DISPLAY_HEIGHT);
		const int x1 = (int)floor(p1.x * DISPLAY_WIDTH);
		const int y1 = (int)floor(p1.y * DISPLAY_HEIGHT);
		if(x0 <= x && x <= x1 && (abs(y0 - y) <= 1 || abs(y1 - y) <= 1)){ return float4(0, 1, 0, 1); }
		if(y0 <= y && y <= y1 && (abs(x0 - x) <= 1 || abs(x1 - x) <= 1)){ return float4(0, 1, 0, 1); }
	}
	return dst;
}

// Grids
float computeHorizontalGridAlpha(float2 uv, float p, float w){
	const float d = abs(uv.y * DISPLAY_HEIGHT - round(p * DISPLAY_HEIGHT));
	return d < w;
}

float computeVerticalGridAlpha(float2 uv, float p, float w){
	const float d = abs(uv.x * DISPLAY_WIDTH - round(p * DISPLAY_WIDTH));
	return d < w;
}

float computeDiagonalGridAlpha(float2 uv, float w, float flip){
	const float2 p0 = uv * float2(DISPLAY_WIDTH, DISPLAY_HEIGHT);
	const float2 p1 = float2(0,             DISPLAY_HEIGHT * flip);
	const float2 p2 = float2(DISPLAY_WIDTH, DISPLAY_HEIGHT * (1.0 - flip));
	return saturate(w + 0.5 - distance(p0, p1, p2));
}

float4 drawGrid3x3(float4 dst, float2 uv, float w, float3 color){
	float alpha = 0.0;
	alpha = max(alpha, computeHorizontalGridAlpha(uv, 1.0 / 3.0, w));
	alpha = max(alpha, computeHorizontalGridAlpha(uv, 2.0 / 3.0, w));
	alpha = max(alpha, computeVerticalGridAlpha(uv, 1.0 / 3.0, w));
	alpha = max(alpha, computeVerticalGridAlpha(uv, 2.0 / 3.0, w));
	return blend(dst, float4(color, alpha));
}

float4 drawGrid3x3Diagonal(float4 dst, float2 uv, float w, float3 color){
	float alpha = 0.0;
	alpha = max(alpha, computeHorizontalGridAlpha(uv, 1.0 / 3.0, w));
	alpha = max(alpha, computeHorizontalGridAlpha(uv, 2.0 / 3.0, w));
	alpha = max(alpha, computeVerticalGridAlpha(uv, 1.0 / 3.0, w));
	alpha = max(alpha, computeVerticalGridAlpha(uv, 2.0 / 3.0, w));
	alpha = max(alpha, computeDiagonalGridAlpha(uv, w, 0.0));
	alpha = max(alpha, computeDiagonalGridAlpha(uv, w, 1.0));
	return blend(dst, float4(color, alpha));
}

float4 drawGrid6x4(float4 dst, float2 uv, float w, float3 color){
	float alpha = 0.0;
	alpha = max(alpha, computeHorizontalGridAlpha(uv, 1.0 / 4.0, w));
	alpha = max(alpha, computeHorizontalGridAlpha(uv, 2.0 / 4.0, w));
	alpha = max(alpha, computeHorizontalGridAlpha(uv, 3.0 / 4.0, w));
	alpha = max(alpha, computeVerticalGridAlpha(uv, 1.0 / 6.0, w));
	alpha = max(alpha, computeVerticalGridAlpha(uv, 2.0 / 6.0, w));
	alpha = max(alpha, computeVerticalGridAlpha(uv, 3.0 / 6.0, w));
	alpha = max(alpha, computeVerticalGridAlpha(uv, 4.0 / 6.0, w));
	alpha = max(alpha, computeVerticalGridAlpha(uv, 5.0 / 6.0, w));
	return blend(dst, float4(color, alpha));
}

float4 drawGrid(float4 dst, float2 uv){
	const int type = (int)round(_GridType);
	if(type == GRID_3X3){
		dst = drawGrid3x3(dst, uv, 3.0, float3(0, 0, 0));
		dst = drawGrid3x3(dst, uv, 1.0, float3(1, 1, 1));
	}else if(type == GRID_3X3_DIAGONAL){
		dst = drawGrid3x3Diagonal(dst, uv, 3.0, float3(0, 0, 0));
		dst = drawGrid3x3Diagonal(dst, uv, 1.0, float3(1, 1, 1));
	}else if(type == GRID_6X4){
		dst = drawGrid6x4(dst, uv, 3.0, float3(0, 0, 0));
		dst = drawGrid6x4(dst, uv, 1.0, float3(1, 1, 1));
	}else if(type == GRID_CUSTOM0){
		dst = blend(dst, _CustomGrid0Tex.Sample(point_clamp_sampler, uv));
	}else if(type == GRID_CUSTOM1){
		dst = blend(dst, _CustomGrid1Tex.Sample(point_clamp_sampler, uv));
	}else if(type == GRID_CUSTOM2){
		dst = blend(dst, _CustomGrid2Tex.Sample(point_clamp_sampler, uv));
	}else if(type == GRID_CUSTOM3){
		dst = blend(dst, _CustomGrid3Tex.Sample(point_clamp_sampler, uv));
	}
	return dst;
}

// Cursor
float2 getFixedFocusPoint(){
	const float2 scale  = SCREEN_ENLARGEMENT;
	const float2 offset = 0.5 * (1.0 - scale); 
	const float2 raw = getFocusPointState();
	return raw * scale + offset;
}

float4 drawPointCursor(float4 dst, float2 uv){
	const float2 comp_size = float2(512.0, 512.0);
	const float2 comp_lo   = float2(  0.0, 128.0);
	const float2 comp_hi   = float2(128.0, 256.0);
	const float2 comp_wh   = comp_hi - comp_lo;
	const float2 pos       = getFixedFocusPoint() * float2(DISPLAY_WIDTH, DISPLAY_HEIGHT);
	const float2 dst_lo    = pos - 0.5 * comp_wh;
	const float2 dst_hi    = pos + 0.5 * comp_wh;
	const float2 pixel_xy  = uv * float2(DISPLAY_WIDTH, DISPLAY_HEIGHT);
	bool between_x = dst_lo.x <= pixel_xy.x && pixel_xy.x <= dst_hi.x;
	bool between_y = dst_lo.y <= pixel_xy.y && pixel_xy.y <= dst_hi.y;
	if(between_x && between_y){
		const float2 t = (pixel_xy - dst_lo + comp_lo) / comp_size;
		float4 src = _ComponentTex.Sample(point_clamp_sampler, t);
		dst = blend(dst, src);
	}
	return dst;
}

float4 drawCursor(float4 dst, float2 uv){
	const int type = (int)round(_CursorType);
	if(type == CURSOR_POINT){
		return drawPointCursor(dst, uv);
	}else{
		return dst;
	}
}

// Level meter
float4 drawRollMeter(float4 dst, float2 uv, float roll){
	const float2 comp_size = float2(512.0, 512.0);
	const float2 comp_lo   = float2(128.0, 128.0);
	const float2 comp_hi   = float2(512.0, 512.0);
	const float2 comp_wh   = comp_hi - comp_lo;
	const float2 dst_lo    = (float2(DISPLAY_WIDTH, DISPLAY_HEIGHT) - comp_wh) * 0.5;
	const float2 dst_hi    = (float2(DISPLAY_WIDTH, DISPLAY_HEIGHT) + comp_wh) * 0.5;
	const float2 pixel_xy  = uv * float2(DISPLAY_WIDTH, DISPLAY_HEIGHT);
	bool between_x = dst_lo.x <= pixel_xy.x && pixel_xy.x <= dst_hi.x;
	bool between_y = dst_lo.y <= pixel_xy.y && pixel_xy.y <= dst_hi.y;
	if(between_x && between_y){
		const float2 t = (pixel_xy - dst_lo + comp_lo) / comp_size;
		float4 src = _ComponentTex.Sample(point_clamp_sampler, t);
		dst = blend(dst, src);
	}
	const float r = length((uv - 0.5) * float2(DISPLAY_WIDTH, DISPLAY_HEIGHT));
	const float alpha_r = min(saturate(256 - r), saturate(r - 160));
	if(alpha_r > 0){
		const float2 p0 = uv * float2(DISPLAY_WIDTH, DISPLAY_HEIGHT);
		const float2 p1 = float2(DISPLAY_WIDTH, DISPLAY_HEIGHT) * 0.5;
		const float2 p2 = float2(cos(-roll), sin(-roll)) + p1;
		const float alpha_d = saturate(2.5 - distance(p0, p1, p2));
		dst = blend(dst, float4(1, 0, 0, alpha_r * alpha_d));
	}
	return dst;
}

float4 drawPitchMeter(float4 dst, float2 uv, float roll, float pitch){
	const float2 comp_size = float2(512.0, 512.0);
	const float2 comp_lo   = float2(  0.0, 320.0);
	const float2 comp_hi   = float2( 48.0, 512.0);
	const float2 comp_wh   = comp_hi - comp_lo;
	const float2 dir = float2(cos(-roll), sin(-roll));
	const float2 rel = float2(DISPLAY_WIDTH * (uv.x - 0.5), DISPLAY_HEIGHT * (uv.y - 0.5));
	const float x = abs(dot(dir, rel)) - 0.5 * (comp_wh.y - comp_wh.x);
	const float y = -dot(float2(dir.y, -dir.x), rel) + 0.5 * comp_wh.y;
	if(x <= comp_wh.x && 0 <= y && y <= comp_wh.y){
		const float2 t = (float2(x, y) + comp_lo) / comp_size;
		float4 src = _ComponentTex.Sample(linear_clamp_sampler, t);
		dst = blend(dst, src);
	}
	const float alpha_x = min(saturate(64 - x), saturate(x - 8));
	const float alpha_y = saturate(2.5 - abs(y - (pitch / PI + 0.5) * comp_wh.y));
	dst = blend(dst, float4(0, 1, 0, alpha_x * alpha_y));
	return dst;
}

// Entry point
float4 fragment(v2f i) : SV_Target {
	const int mode = (int)round(_PeakingMode);
	float4 output = drawCapturedImage(i.uv);
	if(mode == 2 || mode == 1 && _LogFocusDistance > _FocusingThresh){
		output = drawFocusPeaking(output, i.uv, i.focus);
	}
	output = drawGrid(output, i.uv);
	output = drawAutoFocusMarker(output, i.uv);
	output = drawCursor(output, i.uv);
	const int show_level = (int)round(_ShowLevel);
	if(show_level){
		output = drawRollMeter(output, i.uv, i.angles.x);
		output = drawPitchMeter(output, i.uv, i.angles.x, i.angles.y);
	}
	return output;
}

#endif
