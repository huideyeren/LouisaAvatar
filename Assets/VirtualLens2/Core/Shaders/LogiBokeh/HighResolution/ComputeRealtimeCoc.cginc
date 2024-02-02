#ifndef VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_COMPUTE_REALTIME_COC_CGINC
#define VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_COMPUTE_REALTIME_COC_CGINC

#include "UnityCG.cginc"
#include "../../Common/Constants.cginc"
#include "../../Common/TargetDetector.cginc"
#include "../../Common/MultiSamplingHelper.cginc"
#include "../Common/Samplers.cginc"

#include "RingKernel.cginc"
#include "../Common/ComputeCoc.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

float _Blurring;

TEXTURE2DMS<float> _DepthTex;


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
	float3 params : TEXCOORD1;
};


//---------------------------------------------------------------------------
// Vertex shader
//---------------------------------------------------------------------------

v2f vertex(appdata v){
	v2f o;

	if(_Blurring == 0.0 || !isVirtualLensCustomComputeCamera(4096, 4608)){
		o.vertex = 0;
		o.uv     = 0;
		o.params = 0;
		return o;
	}

	const float x = v.uv.x * 0.75;
	const float y = v.uv.y * 0.25 + 0.75;

	const float proj_x = (x * 2 - 1);
	const float proj_y = (y * 2 - 1) * _ProjectionParams.x;
	o.vertex = float4(proj_x, proj_y, 0, 1);
	o.uv     = float2(x, y);
	o.params = computeCocParameters(_DepthTex, float2(1920.0, 1080.0));
	return o;
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

float computeFull(float2 uv, float3 params){
	// TODO full quality reduction?
	int width = 0, height = 0, num_samples = 0;
	TEXTURE2DMS_GET_DIMENSIONS(_DepthTex, width, height, num_samples);

	const int2 coord = (int2)floor(uv * float2(width, height) + float2(-0.5, 0.5));
	const float coc = computeCoc(_DepthTex.Load(TEXTURE2DMS_COORD(coord), 0), params);
	const float w   = saturate(1.5 - abs(coc));

	if(w == 0.0){
		return 0.0;
	}else{
		return coc / w;
	}
}


float computeHalfBackground(float2 uv, float3 params){
	// TODO full quality reduction?
	int width = 0, height = 0, num_samples = 0;
	TEXTURE2DMS_GET_DIMENSIONS(_DepthTex, width, height, num_samples);

	const int2 coord00 = (int2)floor(uv * float2(width, height) + float2(-1.5, 1.5));
	const int2 coord01 = coord00 + int2(2,  0);
	const int2 coord10 = coord00 + int2(0, -2);
	const int2 coord11 = coord00 + int2(2,  0);

	const float4 cocs = float4(
		computeCoc(_DepthTex.Load(TEXTURE2DMS_COORD(coord00), 0), params),
		computeCoc(_DepthTex.Load(TEXTURE2DMS_COORD(coord01), 0), params),
		computeCoc(_DepthTex.Load(TEXTURE2DMS_COORD(coord10), 0), params),
		computeCoc(_DepthTex.Load(TEXTURE2DMS_COORD(coord11), 0), params));
	return max(max(max(cocs.x, cocs.y), max(cocs.z, cocs.w)), -1e-3);
}


float computeHalfForeground(float2 uv, float3 params){
	// TODO full quality reduction?
	int width = 0, height = 0, num_samples = 0;
	TEXTURE2DMS_GET_DIMENSIONS(_DepthTex, width, height, num_samples);

	const int2 coord00 = (int2)floor(uv * float2(width, height) + float2(-1.5, 1.5));
	const int2 coord01 = coord00 + int2(2,  0);
	const int2 coord10 = coord00 + int2(0, -2);
	const int2 coord11 = coord00 + int2(2,  0);

	const float4 cocs = float4(
		-computeCoc(_DepthTex.Load(TEXTURE2DMS_COORD(coord00), 0), params),
		-computeCoc(_DepthTex.Load(TEXTURE2DMS_COORD(coord01), 0), params),
		-computeCoc(_DepthTex.Load(TEXTURE2DMS_COORD(coord10), 0), params),
		-computeCoc(_DepthTex.Load(TEXTURE2DMS_COORD(coord11), 0), params));
	return max(max(max(cocs.x, cocs.y), max(cocs.z, cocs.w)), -1e-3);
}


float fragment(v2f i) : SV_Target {
	if(i.uv.x < 0.5){
		return computeFull(i.uv * float2(2.0, 4.0) - float2(0.0, 3.0), i.params);
	}else if(i.uv.y < 0.875){
		return computeHalfBackground(i.uv * float2(4.0, 8.0) - float2(2.0, 6.0), i.params);
	}else{
		return computeHalfForeground(i.uv * float2(4.0, 8.0) - float2(2.0, 7.0), i.params);
	}
}


#endif
