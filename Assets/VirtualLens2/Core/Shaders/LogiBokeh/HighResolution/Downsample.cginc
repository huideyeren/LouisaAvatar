#ifndef VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_DOWNSAMPLE_CGINC
#define VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_DOWNSAMPLE_CGINC

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

TEXTURE2DMS<float3> _MainTex;
TEXTURE2DMS<float>  _DepthTex;


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

	if(!isVirtualLensCustomComputeCamera(4096, 4608)){
		o.vertex = 0;
		o.uv     = 0;
		o.params = 0;
		return o;
	}

	const float x = v.uv.x;
	const float y = v.uv.y * 0.25 + 0.75;

	const float proj_x = (x * 2 - 1);
	const float proj_y = (y * 2 - 1) * _ProjectionParams.x;
	o.vertex = float4(proj_x, proj_y, 0, 1);
	o.uv     = v.uv;
	o.params = computeCocParameters(_DepthTex, float2(1920.0, 1080.0));
	return o;
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

float4 fragment(v2f i) : SV_Target {
	// TODO full quality downsampling?
	int width, height, num_samples;
	TEXTURE2DMS_GET_DIMENSIONS(_MainTex, width, height, num_samples);

	if(i.uv.x < 0.5){
		// Raw downsampled (RGB-CoC)
		const float2 uv = i.uv * float2(2.0, 1.0) - float2(0.0, 0.0);
		const int2 coord = (int2)floor(uv * float2(width, height) + float2(-0.5, 0.5));
		// max(x, 0): avoiding NaN
		const float3 rgb = max(_MainTex.Load(TEXTURE2DMS_COORD(coord), 0), 0.0);
		const float  coc = computeCoc(_DepthTex.Load(TEXTURE2DMS_COORD(coord), 0), i.params);
		return float4(rgb, coc);
	}else{
		// Premultiplied
		const float2 uv = i.uv * float2(2.0, 1.0) - float2(1.0, 0.0);
		const int2 coord = (int2)floor(uv * float2(width, height) + float2(-0.5, 0.5));
		// max(x, 0): avoiding NaN
		const float3 rgb = max(_MainTex.Load(TEXTURE2DMS_COORD(coord), 0), 0.0);
		const float  coc = computeCoc(_DepthTex.Load(TEXTURE2DMS_COORD(coord), 0), i.params);
		const float  w   = saturate(1.5 - abs(coc));
		return float4(rgb * w, w);
	}
}


#endif
