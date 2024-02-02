#ifndef VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_REALTIME_COC_CONFIG_CGINC
#define VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_REALTIME_COC_CONFIG_CGINC

#include "../Common/Samplers.cginc"

/* _CocTex layout
 *
 *   +---------------+-------+-------+
 *   |               | RT BG |       |
 *   |    RT Full    +-------+       |
 *   |               | RT FG |       |
 *   +---------------+-------+-------+
 *   |               |               |
 *   |     HR BG     |     HR FG     |
 *   |               |               |
 *   +---------------+---------------+
 *   |                               |
 *   |                               |
 *   |                               |
 *   |            HR Full            |
 *   |                               |
 *   |                               |
 *   |                               |
 *   +-------------------------------+
 *
 *  - HR Full: 4096 x 2304
 *  - HR BG:   2048 x 1152
 *  - HR FG:   2048 x 1152
 *  - RT Full: 2048 x 1152
 *  - RT BG:   1024 x  576
 *  - RT FG:   1024 x  576
 */

static const float2 FULL_COC_TEXEL_SIZE = float2(1.0 / 2048.0, 1.0 / 1152.0);
static const float2 HALF_COC_TEXEL_SIZE = float2(1.0 / 1024.0, 1.0 /  576.0);

Texture2D _CocTex;


float fetchFullCoc(float2 uv){
	const float2 minval = float2(0.0, 0.75);
	const float2 maxval = float2(0.5, 1.0);
	const float2 scale  = maxval - minval;
	const float2 offset = minval;
	return _CocTex.SampleLevel(point_clamp_sampler, uv * scale + offset, 0);
}

float4 gatherFullCoc(float2 uv){
	const float2 minval = float2(0.0, 0.75);
	const float2 maxval = float2(0.5, 1.0);
	const float2 scale  = maxval - minval;
	const float2 offset = minval;
	return _CocTex.Gather(point_clamp_sampler, uv * scale + offset);
}


float fetchBackgroundCoc(float2 uv){
	const float2 minval = float2(0.5,  0.75);
	const float2 maxval = float2(0.75, 0.875);
	const float2 scale  = maxval - minval;
	const float2 offset = minval;
	return _CocTex.SampleLevel(point_clamp_sampler, uv * scale + offset, 0);
}

float fetchBackgroundCocLinear(float2 uv){
	const float2 minval = float2(0.5,  0.75);
	const float2 maxval = float2(0.75, 0.875);
	const float2 scale  = maxval - minval;
	const float2 offset = minval;
	return _CocTex.SampleLevel(linear_clamp_sampler, uv * scale + offset, 0);
}

float4 gatherBackgroundCoc(float2 uv){
	const float2 minval = float2(0.5,  0.75);
	const float2 maxval = float2(0.75, 0.875);
	const float2 scale  = maxval - minval;
	const float2 offset = minval;
	return _CocTex.Gather(point_clamp_sampler, uv * scale + offset);
}


float fetchForegroundCoc(float2 uv){
	const float2 minval = float2(0.5,  0.875);
	const float2 maxval = float2(0.75, 1.0);
	const float2 scale  = maxval - minval;
	const float2 offset = minval;
	return _CocTex.SampleLevel(point_clamp_sampler, uv * scale + offset, 0);
}

float fetchForegroundCocLinear(float2 uv){
	const float2 minval = float2(0.5,  0.875);
	const float2 maxval = float2(0.75, 1.0);
	const float2 scale  = maxval - minval;
	const float2 offset = minval;
	return _CocTex.SampleLevel(linear_clamp_sampler, uv * scale + offset, 0);
}

float4 gatherForegroundCoc(float2 uv){
	const float2 minval = float2(0.5,  0.875);
	const float2 maxval = float2(0.75, 1.0);
	const float2 scale  = maxval - minval;
	const float2 offset = minval;
	return _CocTex.Gather(point_clamp_sampler, uv * scale + offset);
}

#endif
