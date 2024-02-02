#ifndef VIRTUALLENS2_DEBUG_RGBD_MULTIPLEXER_CGINC
#define VIRTUALLENS2_DEBUG_RGBD_MULTIPLEXER_CGINC

#include "UnityCG.cginc"
#include "../Common/TargetDetector.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

float _Near;
float _Far;

Texture2D _MainTex;
Texture2D _DepthTex;

SamplerState point_clamp_sampler;


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
};


//---------------------------------------------------------------------------
// Vertex shader
//---------------------------------------------------------------------------

v2f vertex(appdata v){
	v2f o;

	if(!isVirtualLensCustomComputeCamera(4096, 2304)){
		o.vertex = 0;
		o.uv     = 0;
		return o;
	}

	const float x = v.uv.x;
	const float y = v.uv.y;

	const float proj_x = (x * 2 - 1);
	const float proj_y = (y * 2 - 1) * _ProjectionParams.x;

	o.vertex = float4(proj_x, proj_y, 0, 1);
	o.uv     = v.uv;
	return o;
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

float linearizeDepth(float d){
	const float z = 1.0 / _Far - 1.0 / _Near;
	const float w = 1.0 / _Near;
	return 1.0 / (z * (1.0 - d) + w);
}

float4 fragment(v2f i) : SV_Target {
	const float3 rgb = _MainTex.Sample(point_clamp_sampler, i.uv);
	const float  d   = _DepthTex.Sample(point_clamp_sampler, i.uv);
	return float4(rgb, linearizeDepth(d));
}

#endif
