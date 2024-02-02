#ifndef VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_REALTIME_SMAA_CGINC
#define VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_REALTIME_SMAA_CGINC

#define SMAA_RT_METRICS   (_ScreenParams.zwxy - float4(1, 1, 0, 0))
#define SMAA_HLSL_4_1     (1)
#define SMAA_PRESET_ULTRA (1)

#define SMAA_AREATEX_SELECT(sample)   (sample.rg)
#define SMAA_SEARCHTEX_SELECT(sample) (sample.a)

SamplerState point_clamp_sampler;
SamplerState linear_clamp_sampler;

#define SMAA_LINEAR_SAMPLER (linear_clamp_sampler)
#define SMAA_POINT_SAMPLER  (point_clamp_sampler)

#include "UnityCG.cginc"
#include "../../SMAA/SMAA.cginc"


//---------------------------------------------------------------------------
// Structure definitions
//---------------------------------------------------------------------------

struct appdata {
	float4 vertex : POSITION;
	float2 uv     : TEXCOORD0;
};


//---------------------------------------------------------------------------
// Utility functions
//---------------------------------------------------------------------------

float4 computeVertexPosition(appdata v){
	const float x = v.uv.x;
	const float y = v.uv.y;

	const float proj_x = (x * 2 - 1);
	const float proj_y = (y * 2 - 1) * _ProjectionParams.x;

	return float4(proj_x, proj_y, 0, 1);
}

float2 computeVertexTexCoord(appdata v){
	return v.uv;
}


#endif
