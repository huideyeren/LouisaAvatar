#ifndef VIRTUALLENS2_DEBUG_DEPTH_EXTRACTOR_CGINC
#define VIRTUALLENS2_DEBUG_DEPTH_EXTRACTOR_CGINC

#include "UnityCG.cginc"
#include "../Common/TargetDetector.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

float _Near;
float _Far;

Texture2D _MainTex;
float4 _MainTex_TexelSize;

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

	if(!isVirtualLensComputeCamera()){
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

float fragment(v2f i) : SV_Depth {
	const float2 texel_size = abs(_MainTex_TexelSize.xy);
	const float  d = _MainTex.Sample(point_clamp_sampler, i.uv + texel_size * float2(-0.25, 0.25)).a;
	const float  z = 1.0 / _Far - 1.0 / _Near;
	const float  w = 1.0 / _Near;
	return 1.0 - 1.0 / (d * z) + w / z;
}

#endif