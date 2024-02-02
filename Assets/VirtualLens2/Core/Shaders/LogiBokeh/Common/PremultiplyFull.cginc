#ifndef VIRTUALLENS2_LOGIBOKEH_COMMON_PREMULTIPLY_FULL_CGINC
#define VIRTUALLENS2_LOGIBOKEH_COMMON_PREMULTIPLY_FULL_CGINC

#include "../../Common/MultiSamplingHelper.cginc"
#include "ComputeCoc.cginc"

//---------------------------------------------------------------------------
// Utility functions
//---------------------------------------------------------------------------

float4 premultiplyFull(
	TEXTURE2DMS<float3> color_tex,
	TEXTURE2DMS<float>  depth_tex,
	float2 uv,
	float3 params)
{
	int width = 0, height = 0, num_samples = 0;
	TEXTURE2DMS_GET_DIMENSIONS(color_tex, width, height, num_samples);

	const int2 coord = (int2)floor(uv * float2(width, height));
	float4 acc = 0.0;

	for(int i = 0; i < num_samples; ++i){
		const float3 rgb   = max(color_tex.Load(TEXTURE2DMS_COORD(coord), i), 0.0);
		const float  depth = depth_tex.Load(TEXTURE2DMS_COORD(coord), i);
		const float  coc   = computeCoc(depth, params);
		const float  w     = saturate(1.5 - abs(coc));
		acc += float4(rgb * w, w);
	}

	return acc * rcp((float)num_samples);
}

#endif
