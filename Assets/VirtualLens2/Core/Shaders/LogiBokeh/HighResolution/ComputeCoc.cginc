#ifndef VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_COMPUTE_COC_CGINC
#define VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_COMPUTE_COC_CGINC

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

	const float x = v.uv.x;
	const float y = v.uv.y * 0.75;

	const float proj_x = (x * 2 - 1);
	const float proj_y = (y * 2 - 1) * _ProjectionParams.x;
	o.vertex = float4(proj_x, proj_y, 0, 1);
	o.uv     = float2(x, y);
	o.params = computeCocParameters(_DepthTex, float2(3840.0, 2160.0));
	return o;
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

float computeFull(float2 uv, float3 params){
	int width = 0, height = 0, num_samples = 0;
	TEXTURE2DMS_GET_DIMENSIONS(_DepthTex, width, height, num_samples);

	const int2 coord = (int2)floor(uv * float2(width, height));
	float coc_sum = 0.0, w_sum = 0.0;
	[unroll(8)]
	for(int i = 0; i < num_samples; ++i){
		const float coc = computeCoc(_DepthTex.Load(TEXTURE2DMS_COORD(coord), i), params);
		const float w   = saturate(1.5 - abs(coc));
		coc_sum += w * coc;
		w_sum   += w;
	}

	if(w_sum == 0.0){
		return 0.0;
	}else{
		return coc_sum / w_sum;
	}
}


float computeHalfBackground(float2 uv, float3 params){
	int width = 0, height = 0, num_samples = 0;
	TEXTURE2DMS_GET_DIMENSIONS(_DepthTex, width, height, num_samples);

	const int2 coord00 = (int2)floor(uv * float2(width, height) - 0.5);
	const int2 coord01 = coord00 + int2(1, 0);
	const int2 coord10 = coord00 + int2(0, 1);
	const int2 coord11 = coord00 + int2(1, 1);

	float max_coc = -1e-3;
	[unroll(8)]
	for(int i = 0; i < num_samples; ++i){
		max_coc = max(max_coc, computeCoc(_DepthTex.Load(TEXTURE2DMS_COORD(coord00), i), params));
		max_coc = max(max_coc, computeCoc(_DepthTex.Load(TEXTURE2DMS_COORD(coord01), i), params));
		max_coc = max(max_coc, computeCoc(_DepthTex.Load(TEXTURE2DMS_COORD(coord10), i), params));
		max_coc = max(max_coc, computeCoc(_DepthTex.Load(TEXTURE2DMS_COORD(coord11), i), params));
	}
	return max_coc;
}


float computeHalfForeground(float2 uv, float3 params){
	int width = 0, height = 0, num_samples = 0;
	TEXTURE2DMS_GET_DIMENSIONS(_DepthTex, width, height, num_samples);

	const int2 coord00 = (int2)floor(uv * float2(width, height) - 0.5);
	const int2 coord01 = coord00 + int2(1, 0);
	const int2 coord10 = coord00 + int2(0, 1);
	const int2 coord11 = coord00 + int2(1, 1);

	float max_coc = -1e-3;
	[unroll(8)]
	for(int i = 0; i < num_samples; ++i){
		max_coc = max(max_coc, -computeCoc(_DepthTex.Load(TEXTURE2DMS_COORD(coord00), i), params));
		max_coc = max(max_coc, -computeCoc(_DepthTex.Load(TEXTURE2DMS_COORD(coord01), i), params));
		max_coc = max(max_coc, -computeCoc(_DepthTex.Load(TEXTURE2DMS_COORD(coord10), i), params));
		max_coc = max(max_coc, -computeCoc(_DepthTex.Load(TEXTURE2DMS_COORD(coord11), i), params));
	}
	return max_coc;
}


float fragment(v2f i) : SV_Target {
	if(i.uv.y < 0.5){
		return computeFull(i.uv * float2(1.0, 2.0), i.params);
	}else if(i.uv.x < 0.5){
		return computeHalfBackground(i.uv * float2(2.0, 4.0) - float2(0.0, 2.0), i.params);
	}else{
		return computeHalfForeground(i.uv * float2(2.0, 4.0) - float2(1.0, 2.0), i.params);
	}
}


#endif
