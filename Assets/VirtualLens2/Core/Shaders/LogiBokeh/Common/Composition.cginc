#ifndef VIRTUALLENS2_LOGIBOKEH_COMMON_COMPOSITION_CGINC
#define VIRTUALLENS2_LOGIBOKEH_COMMON_COMPOSITION_CGINC

#include "../../Common/MultiSamplingHelper.cginc"
#include "Samplers.cginc"
#include "Utility.cginc"

#ifdef FUSED_POSTFILTERING
#include "Postfilter.cginc"
#endif


//---------------------------------------------------------------------------
// Utility functions
//---------------------------------------------------------------------------

float4 fetchBlurredBackground(float2 uv){
#ifdef FUSED_POSTFILTERING
	const float2 uv_pixels     = uv * rcp(PREMULTIPLIED_FULL_TEXEL_SIZE);
	const float2 subpix_offset = uv_pixels - floor(uv_pixels);
	const float2 signs         = subpix_offset * 4.0 - 4.0;
	const float2 center_uv     = uv - 0.25 * WORKAREA_TEXEL_SIZE * signs;
	const float2 horizontal_uv = center_uv + float2(signs.x, 0.0) * WORKAREA_TEXEL_SIZE;
	const float2 vertical_uv   = center_uv + float2(0.0, signs.y) * WORKAREA_TEXEL_SIZE;
	const float2 diagonal_uv   = center_uv + signs * WORKAREA_TEXEL_SIZE;
	float4 acc = 0.0;
	acc += (9.0 / 16.0) * backgroundPostfilter(center_uv,     rawBackgroundWorkareaUV(center_uv));
	acc += (3.0 / 16.0) * backgroundPostfilter(horizontal_uv, rawBackgroundWorkareaUV(horizontal_uv));
	acc += (3.0 / 16.0) * backgroundPostfilter(vertical_uv,   rawBackgroundWorkareaUV(vertical_uv));
	acc += (1.0 / 16.0) * backgroundPostfilter(diagonal_uv,   rawBackgroundWorkareaUV(diagonal_uv));
	return acc;
#else
	return fetchBackgroundWorkarea(linear_clamp_sampler, uv);
#endif
}

float4 fetchBlurredForeground(float2 uv){
#ifdef FUSED_POSTFILTERING
	const float2 uv_pixels     = uv * rcp(PREMULTIPLIED_FULL_TEXEL_SIZE);
	const float2 subpix_offset = uv_pixels - floor(uv_pixels);
	const float2 signs         = subpix_offset * 4.0 - 4.0;
	const float2 center_uv     = uv - 0.25 * WORKAREA_TEXEL_SIZE * signs;
	const float2 horizontal_uv = center_uv + float2(signs.x, 0.0) * WORKAREA_TEXEL_SIZE;
	const float2 vertical_uv   = center_uv + float2(0.0, signs.y) * WORKAREA_TEXEL_SIZE;
	const float2 diagonal_uv   = center_uv + signs * WORKAREA_TEXEL_SIZE;
	float4 acc = 0.0;
	acc += (9.0 / 16.0) * foregroundPostfilter(center_uv,     rawForegroundWorkareaUV(center_uv));
	acc += (3.0 / 16.0) * foregroundPostfilter(horizontal_uv, rawForegroundWorkareaUV(horizontal_uv));
	acc += (3.0 / 16.0) * foregroundPostfilter(vertical_uv,   rawForegroundWorkareaUV(vertical_uv));
	acc += (1.0 / 16.0) * foregroundPostfilter(diagonal_uv,   rawForegroundWorkareaUV(diagonal_uv));
	return acc;
#else
	return fetchForegroundWorkarea(linear_clamp_sampler, uv);
#endif
}


void accumulateFullResolution(
	inout float4 acc,
	inout float  max_backgroundness,
	float2 offset,
	float4 rgbm,
	float  raw_coc,
	float  mirror_coc)
{
	const float dist         = length(offset);
	const float mirror_sel   = 2.0 * saturate(-2.0 * raw_coc);
	const float coc          = raw_coc * (2.0 - mirror_sel) - min(raw_coc, mirror_coc) * mirror_sel;
	const float intersection = saturate(coc - dist + 0.5);
	const float weight       = sample_alpha(coc) * intersection;
	acc += rgbm * weight;
	if(raw_coc >= 0.0){
		max_backgroundness = max(
			max_backgroundness,
			saturate(1.0 - rgbm.a) + 1.0 - dist);
	}
}


float4 gatherFullResolution(float2 uv){
	// . 0 0 A .
	// D 0 0 1 1
	// 3 3 X 1 1
	// 3 3 2 2 B
	// . C 2 2 .

	const float2 texel_size = PREMULTIPLIED_FULL_TEXEL_SIZE;

	float4 acc = 0.0;
	float  max_backgroundness = 0.0;

	{	// Center
		const float4 rgbm    = fetchPremultipliedFullSample(linear_clamp_sampler, uv);
		const float  raw_coc = fetchFullCoc(uv);
		const float  coc     = raw_coc * 2.0;
		const float  weight  = sample_alpha(coc);
		acc = rgbm * weight;
		if(raw_coc >= 0.0){
			max_backgroundness = saturate(1.0 - rgbm.a) + 1.0;
		}
	}

	{	// 0+2
		const float2 offset0_w = float2(-1, -2), offset0_z = float2( 0, -2);
		const float2 offset0_x = float2(-1, -1), offset0_y = float2( 0, -1);
		const float2 offset0   = (offset0_w + offset0_z + offset0_x + offset0_y) * 0.25;
		const float2 offset2_w = float2( 0,  1), offset2_z = float2( 1,  1);
		const float2 offset2_x = float2( 0,  2), offset2_y = float2( 1,  2);
		const float2 offset2   = (offset2_w + offset2_z + offset2_x + offset2_y) * 0.25;
		const float4 raw_cocs0 = gatherFullCoc(uv + offset0 * texel_size);
		const float4 raw_cocs2 = gatherFullCoc(uv + offset2 * texel_size);
		const float4 rgb0_w    = fetchPremultipliedFullSample(linear_clamp_sampler, uv + offset0_w * texel_size);
		accumulateFullResolution(acc, max_backgroundness, offset0_w, rgb0_w, raw_cocs0.w, raw_cocs2.y);
		const float4 rgb0_z    = fetchPremultipliedFullSample(linear_clamp_sampler, uv + offset0_z * texel_size);
		accumulateFullResolution(acc, max_backgroundness, offset0_z, rgb0_z, raw_cocs0.z, raw_cocs2.x);
		const float4 rgb0_x    = fetchPremultipliedFullSample(linear_clamp_sampler, uv + offset0_x * texel_size);
		accumulateFullResolution(acc, max_backgroundness, offset0_x, rgb0_x, raw_cocs0.x, raw_cocs2.z);
		const float4 rgb0_y    = fetchPremultipliedFullSample(linear_clamp_sampler, uv + offset0_y * texel_size);
		accumulateFullResolution(acc, max_backgroundness, offset0_y, rgb0_y, raw_cocs0.y, raw_cocs2.w);
		const float4 rgb2_w    = fetchPremultipliedFullSample(linear_clamp_sampler, uv + offset2_w * texel_size);
		accumulateFullResolution(acc, max_backgroundness, offset2_w, rgb2_w, raw_cocs2.w, raw_cocs0.y);
		const float4 rgb2_z    = fetchPremultipliedFullSample(linear_clamp_sampler, uv + offset2_z * texel_size);
		accumulateFullResolution(acc, max_backgroundness, offset2_z, rgb2_z, raw_cocs2.z, raw_cocs0.x);
		const float4 rgb2_x    = fetchPremultipliedFullSample(linear_clamp_sampler, uv + offset2_x * texel_size);
		accumulateFullResolution(acc, max_backgroundness, offset2_x, rgb2_x, raw_cocs2.x, raw_cocs0.z);
		const float4 rgb2_y    = fetchPremultipliedFullSample(linear_clamp_sampler, uv + offset2_y * texel_size);
		accumulateFullResolution(acc, max_backgroundness, offset2_y, rgb2_y, raw_cocs2.y, raw_cocs0.w);
	}
	{	// 1+3
		const float2 offset1_w = float2( 1, -1), offset1_z = float2( 2, -1);
		const float2 offset1_x = float2( 1,  0), offset1_y = float2( 2,  0);
		const float2 offset1   = (offset1_w + offset1_z + offset1_x + offset1_y) * 0.25;
		const float2 offset3_w = float2(-2,  0), offset3_z = float2(-1,  0);
		const float2 offset3_x = float2(-2,  1), offset3_y = float2(-1,  1);
		const float2 offset3   = (offset3_w + offset3_z + offset3_x + offset3_y) * 0.25;
		const float4 raw_cocs1 = gatherFullCoc(uv + offset1 * texel_size);
		const float4 raw_cocs3 = gatherFullCoc(uv + offset3 * texel_size);
		const float4 rgb1_w    = fetchPremultipliedFullSample(linear_clamp_sampler, uv + offset1_w * texel_size);
		accumulateFullResolution(acc, max_backgroundness, offset1_w, rgb1_w, raw_cocs1.w, raw_cocs3.y);
		const float4 rgb1_z    = fetchPremultipliedFullSample(linear_clamp_sampler, uv + offset1_z * texel_size);
		accumulateFullResolution(acc, max_backgroundness, offset1_z, rgb1_z, raw_cocs1.z, raw_cocs3.x);
		const float4 rgb1_x    = fetchPremultipliedFullSample(linear_clamp_sampler, uv + offset1_x * texel_size);
		accumulateFullResolution(acc, max_backgroundness, offset1_x, rgb1_x, raw_cocs1.x, raw_cocs3.z);
		const float4 rgb1_y    = fetchPremultipliedFullSample(linear_clamp_sampler, uv + offset1_y * texel_size);
		accumulateFullResolution(acc, max_backgroundness, offset1_y, rgb1_y, raw_cocs1.y, raw_cocs3.w);
		const float4 rgb3_w    = fetchPremultipliedFullSample(linear_clamp_sampler, uv + offset3_w * texel_size);
		accumulateFullResolution(acc, max_backgroundness, offset3_w, rgb3_w, raw_cocs3.w, raw_cocs1.y);
		const float4 rgb3_z    = fetchPremultipliedFullSample(linear_clamp_sampler, uv + offset3_z * texel_size);
		accumulateFullResolution(acc, max_backgroundness, offset3_z, rgb3_z, raw_cocs3.z, raw_cocs1.x);
		const float4 rgb3_x    = fetchPremultipliedFullSample(linear_clamp_sampler, uv + offset3_x * texel_size);
		accumulateFullResolution(acc, max_backgroundness, offset3_x, rgb3_x, raw_cocs3.x, raw_cocs1.z);
		const float4 rgb3_y    = fetchPremultipliedFullSample(linear_clamp_sampler, uv + offset3_y * texel_size);
		accumulateFullResolution(acc, max_backgroundness, offset3_y, rgb3_y, raw_cocs3.y, raw_cocs1.w);
	}
	{	// A+C
		const float2 offsetA  = float2( 1, -2);
		const float2 offsetC  = float2(-1,  2);
		const float4 raw_cocA = fetchFullCoc(uv + offsetA * texel_size);
		const float4 raw_cocC = fetchFullCoc(uv + offsetC * texel_size);
		const float4 rgbA     = fetchPremultipliedFullSample(linear_clamp_sampler, uv + offsetA * texel_size);
		accumulateFullResolution(acc, max_backgroundness, offsetA, rgbA, raw_cocA, raw_cocC);
		const float4 rgbC     = fetchPremultipliedFullSample(linear_clamp_sampler, uv + offsetC * texel_size);
		accumulateFullResolution(acc, max_backgroundness, offsetC, rgbC, raw_cocC, raw_cocA);
	}

	{	// B+D
		const float2 offsetB  = float2( 2,  1);
		const float2 offsetD  = float2(-2, -1);
		const float4 raw_cocB = fetchFullCoc(uv + offsetB * texel_size);
		const float4 raw_cocD = fetchFullCoc(uv + offsetD * texel_size);
		const float4 rgbB     = fetchPremultipliedFullSample(linear_clamp_sampler, uv + offsetB * texel_size);
		accumulateFullResolution(acc, max_backgroundness, offsetB, rgbB, raw_cocB, raw_cocD);
		const float4 rgbD     = fetchPremultipliedFullSample(linear_clamp_sampler, uv + offsetD * texel_size);
		accumulateFullResolution(acc, max_backgroundness, offsetD, rgbD, raw_cocD, raw_cocB);
	}

	if(acc.a > 0.0){
		const float select = saturate(max_backgroundness);
		return float4(acc.rgb / acc.a, select * saturate(acc.a) + (1.0 - select));
	}else{
		return float4(0, 0, 0, 0);
	}
}


float3 approximateHoleFilling(int num_rings, float2 uv){
	const float2 texel_size = WORKAREA_TEXEL_SIZE;

	[loop]
	for(int ring_id = 1; ring_id < num_rings; ++ring_id){
		const float dist       = ring_center_radius(ring_id);
		const float theta_step = ring_angular_step(ring_id);
		const float theta_end  = 0.5 * PI - 0.25 * theta_step;
		float4 acc = 0.0;

		{
			[loop]
			for(float theta = ring_angular_offset(ring_id); theta < theta_end; theta += theta_step){
				const float2 offset = float2(cos(theta), sin(theta)) * dist;
				acc += fetchBackgroundWorkarea(linear_clamp_sampler, uv + texel_size * float2( offset.x,  offset.y));
				acc += fetchBackgroundWorkarea(linear_clamp_sampler, uv + texel_size * float2(-offset.y,  offset.x));
				acc += fetchBackgroundWorkarea(linear_clamp_sampler, uv + texel_size * float2(-offset.x, -offset.y));
				acc += fetchBackgroundWorkarea(linear_clamp_sampler, uv + texel_size * float2( offset.y, -offset.x));
			}
		}

		{
			[loop]
			for(float theta = ring_angular_offset(ring_id); theta < theta_end; theta += theta_step){
				const float2 offset = float2(cos(theta), sin(theta)) * dist;
				const float2 c0 = uv + texel_size * float2( offset.x,  offset.y);
				acc += fetchPremultipliedFullSample(linear_clamp_sampler, c0);
				const float2 c1 = uv + texel_size * float2(-offset.y,  offset.x);
				acc += fetchPremultipliedFullSample(linear_clamp_sampler, c1);
				const float2 c2 = uv + texel_size * float2(-offset.x, -offset.y);
				acc += fetchPremultipliedFullSample(linear_clamp_sampler, c2);
				const float2 c3 = uv + texel_size * float2( offset.y, -offset.x);
				acc += fetchPremultipliedFullSample(linear_clamp_sampler, c3);
			}
		}

		if(acc.a > 0.0){
			return acc.rgb / acc.a;
		}
	}

	return 0.0;
}


float4 composition(int num_rings, float2 uv){
	const float4 fg = fetchBlurredForeground(uv);
	if(fg.a >= 1.0){ return float4(fg.rgb, 1.0); }

	const float4 bg = fetchBlurredBackground(uv);
	float4 result = (bg.a > 0.0)
		? float4(bg.rgb / bg.a, saturate(bg.a * 32.0))
		: float4(0, 0, 0, 0);

	const float4 tile = fetchDilatedTile(uv);
	float4 fc = 0.0;
	if((tile.x < 1.5 && tile.y >= 0.0) || (tile.z < 1.5 && tile.w >= 0.0)){
		fc = gatherFullResolution(uv);
		if(fc.a > 0.0){
			fc.a = 1.0 - (1.0 - fc.a) * saturate(64.0 - 64.0 * saturate(fg.a + fc.a));
		}
	}

	if(fc.a < 1.0 && 0.0 < fg.a && result.a < 1.0){
		const float4 hf = float4(approximateHoleFilling(num_rings, uv), 1.0 - result.a);
		result = alpha_blend(float4(result.rgb, 1.0), hf);
	}

	result = alpha_blend(result, fc);

	result = premultiplied_alpha_blend(result, fg);

	return result;
}


float4 passthrough(TEXTURE2DMS<float3> color_tex, float2 uv)
{
	int width = 0, height = 0, num_samples = 0;
	TEXTURE2DMS_GET_DIMENSIONS(color_tex, width, height, num_samples);

	const int2 coord = (int2)floor(uv * float2(width, height));
	float4 acc = 0.0;

	[unroll(8)]
	for(int i = 0; i < num_samples; ++i){
		const float3 rgb = max(color_tex.Load(TEXTURE2DMS_COORD(coord), i), 0.0);
		acc += float4(rgb, 1.0);
	}

	return acc * rcp((float)num_samples);
}


float4 prepareAntiAliasing(float3 rgb){
#if defined(USE_FXAA)
	// Return linear RGB + gamma luminance
	return float4(rgb, sqrt(rgb2luma(rgb)));
#elif defined(USE_SMAA)
	// Return gamma RGB
	return float4(linear2gamma(rgb), 1.0);
#else
	// Return linear color
	return float4(rgb, 1.0);
#endif
}

#endif
