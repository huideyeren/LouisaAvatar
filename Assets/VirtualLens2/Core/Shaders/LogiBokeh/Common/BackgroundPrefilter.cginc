#ifndef VIRTUALLENS2_LOGIBOKEH_COMMON_BACKGROUND_PREFILTER_CGINC
#define VIRTUALLENS2_LOGIBOKEH_COMMON_BACKGROUND_PREFILTER_CGINC

#include "../../Common/MultiSamplingHelper.cginc"
#include "Samplers.cginc"

#ifndef NO_MULTISAMPLED
#include "ComputeCoc.cginc"
#endif


//---------------------------------------------------------------------------
// Utility functions
//---------------------------------------------------------------------------

#ifdef NO_MULTISAMPLED

float4 downsampleBackground(float2 uv){
	const float2 texel_size = FULL_TEXEL_SIZE;

	const float coc = max(0.0, fetchBackgroundCoc(uv));
	if(coc == 0.0){ return float4(0, 0, 0, 0); }

	const float4 rgbc00 = fetchFullSample(linear_clamp_sampler, uv + texel_size * float2(-0.5, -0.5));
	const float4 rgbc01 = fetchFullSample(linear_clamp_sampler, uv + texel_size * float2( 0.5, -0.5));
	const float4 rgbc10 = fetchFullSample(linear_clamp_sampler, uv + texel_size * float2(-0.5,  0.5));
	const float4 rgbc11 = fetchFullSample(linear_clamp_sampler, uv + texel_size * float2( 0.5,  0.5));

	const float w00 = saturate(rgbc00.a - 0.5);
	const float w01 = saturate(rgbc01.a - 0.5);
	const float w10 = saturate(rgbc10.a - 0.5);
	const float w11 = saturate(rgbc11.a - 0.5);

	const float  w_sum = max(w00 + w01 + w10 + w11, 1e-3);
	const float3 acc = (rgbc00.rgb * w00 + rgbc01.rgb * w01 + rgbc10.rgb * w10 + rgbc11.rgb * w11) / w_sum;

	const float mult = (w00 + w01 + w10 + w11) * 0.25;
	return float4(acc * mult, mult);
}

#else

float4 downsampleBackground(
	TEXTURE2DMS<float3> color_tex,
	TEXTURE2DMS<float>  depth_tex,
	float2 uv,
	float3 params)
{
	const float tile_max = fetchTile(uv).y;
	if(tile_max < 0.5){ return 0.0; }

	int width, height, num_samples;
	TEXTURE2DMS_GET_DIMENSIONS(color_tex, width, height, num_samples);

	const int2 coord00 = (int2)floor(uv * float2(width, height) - 0.5);
	float4 acc = 0.0;

	[unroll]
	for(int offset = 0; offset < 4; ++offset){
		const int2 coord = coord00 + int2(offset & 1, offset >> 1);
		[unroll(8)]
		for(int i = 0; i < num_samples; ++i){
			const float3 color = max(color_tex.Load(TEXTURE2DMS_COORD(coord), i), 0.0);
			const float  coc   = computeCoc(depth_tex.Load(TEXTURE2DMS_COORD(coord), i), params);
			const float  w     = saturate(coc - 0.5);
			acc += float4(color * w, w);
		}
	}
	
	return acc * (0.25 * rcp((float)num_samples));
}

#endif

#endif
