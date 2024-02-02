#ifndef VIRTUALLENS2_LOGIBOKEH_COMMON_FOREGROUND_BLUR_CGINC
#define VIRTUALLENS2_LOGIBOKEH_COMMON_FOREGROUND_BLUR_CGINC

#include "Samplers.cginc"
#include "Utility.cginc"
#include "Configuration.cginc"


//---------------------------------------------------------------------------
// Structure definitions
//---------------------------------------------------------------------------

struct ForegroundBucket {
	float3 color;
	float  weight;
};


//---------------------------------------------------------------------------
// Utility functions
//---------------------------------------------------------------------------

static const int FOREGROUND_NUM_BUCKETS = 4;


void accumulateForeground(
	inout ForegroundBucket buckets[FOREGROUND_NUM_BUCKETS],
	float2 coord,
	float  rcp_intersection_deno,
	float  intersection_offset,
	float  bucket_width)
{
	const float4 fetched      = fetchDownsampledForeground(linear_clamp_sampler, coord);
	const float  coc          = fetchForegroundCocLinear(coord);
	const float3 rgb          = fetched.rgb;
	const float  mult         = fetched.a;
	const float  intersection = saturate(coc * rcp_intersection_deno - intersection_offset);
	const float  weight       = sample_alpha(coc) * intersection * AREA_PER_SAMPLE;
	[unroll]
	for(int j = 0; j < FOREGROUND_NUM_BUCKETS; ++j){
		const float bucket_center = bucket_width * (j + 0.5);
		float bucket_weight = 0.0;
		if(j == 0 && coc < bucket_center){
			bucket_weight = 1.0;
		}else if(j == FOREGROUND_NUM_BUCKETS - 1 && coc > bucket_center){
			bucket_weight = 1.0;
		}else{
			bucket_weight = saturate(1.0 - abs(coc - bucket_center) * rcp(bucket_width));
		}
		buckets[j].color  += weight * bucket_weight * rgb;
		buckets[j].weight += weight * bucket_weight * mult;
	}
}


float4 blurForeground(int num_rings, float2 uv){
	const float2 texel_size   = DOWNSAMPLED_TEXEL_SIZE;
	const float2 tile         = max(0.0, fetchDilatedTile(uv).zw);
	const float  bucket_width = ring_border_radius(num_rings) / FOREGROUND_NUM_BUCKETS;

	// Color only optimization
	if((tile.y - tile.x) < tile.y * 0.05){
		const float coc = (tile.x + tile.y) * 0.5;
		float bucket = 0.0;
		if(coc < bucket_width * 0.5){
			bucket = 0.0;
		}else if(coc > bucket_width * (FOREGROUND_NUM_BUCKETS - 0.5)){
			bucket = 1.0;
		}else{
			const float t = (coc - bucket_width * 0.5) / bucket_width;
			bucket = t - floor(t);
		}

		float4 lo_sum = 0.0, hi_sum = 0.0;
		[loop]
		for(int ring_id = num_rings - 1; ring_id > 0; --ring_id){
			const float inner_radius = ring_border_radius(ring_id);
			const float outer_radius = ring_border_radius(ring_id + 1);
			if(coc < inner_radius){ continue; }
			const float intersection = saturate((coc - inner_radius) * rcp(outer_radius - inner_radius));
			const float weight       = sample_alpha(coc) * intersection * AREA_PER_SAMPLE;
			const float dist         = ring_center_radius(ring_id);
			const float theta_step   = ring_angular_step(ring_id);
			const float theta_end    = 0.5 * PI - 0.25 * theta_step;
			[loop]
			for(float theta = ring_angular_offset(ring_id); theta < theta_end; theta += theta_step){
				const float2 offset   = float2(cos(theta), sin(theta)) * dist;
				const float2 coord0   = uv + texel_size * float2( offset.x,  offset.y);
				const float4 fetched0 = fetchDownsampledForeground(linear_clamp_sampler, coord0);
				lo_sum += weight * (1.0 - bucket) * fetched0;
				hi_sum += weight * bucket         * fetched0;
				const float2 coord1   = uv + texel_size * float2(-offset.y,  offset.x);
				const float4 fetched1 = fetchDownsampledForeground(linear_clamp_sampler, coord1);
				lo_sum += weight * (1.0 - bucket) * fetched1;
				hi_sum += weight * bucket         * fetched1;
				const float2 coord2   = uv + texel_size * float2(-offset.x, -offset.y);
				const float4 fetched2 = fetchDownsampledForeground(linear_clamp_sampler, coord2);
				lo_sum += weight * (1.0 - bucket) * fetched2;
				hi_sum += weight * bucket         * fetched2;
				const float2 coord3   = uv + texel_size * float2( offset.y, -offset.x);
				const float4 fetched3 = fetchDownsampledForeground(linear_clamp_sampler, coord3);
				lo_sum += weight * (1.0 - bucket) * fetched3;
				hi_sum += weight * bucket         * fetched3;
			}
		}
		{	// ring_id == 0
			const float  outer_radius = ring_border_radius(1);
			const float  intersection = saturate(coc * rcp(outer_radius));
			const float  weight       = sample_alpha(coc) * intersection * AREA_PER_SAMPLE;
			const float4 fetched      = fetchDownsampledForeground(linear_clamp_sampler, uv);
			lo_sum += weight * (1.0 - bucket) * fetched;
			hi_sum += weight * bucket         * fetched;
		}

		float4 result = 0.0;
		if(lo_sum.a > 0.0){
			result = float4(lo_sum.rgb / lo_sum.a, saturate(lo_sum.a));
		}
		if(hi_sum.a > 0.0){
			result = alpha_blend(result, float4(hi_sum.rgb / hi_sum.a, saturate(hi_sum.a)));
		}
		const float alpha = saturate(result.a * FOREGROUND_ALPHA_MULTIPLE);
		return float4(result.rgb * alpha, alpha);
	}

	// Scatter-as-gather with fixed bucketing
	ForegroundBucket buckets[FOREGROUND_NUM_BUCKETS];
	{
		for(int i = 0; i < FOREGROUND_NUM_BUCKETS; ++i){
			buckets[i].color  = 0.0;
			buckets[i].weight = 0.0;
		}
	}

	[loop]
	for(int ring_id = num_rings - 1; ring_id > 0; --ring_id){
		const float inner_radius = ring_border_radius(ring_id);
		const float outer_radius = ring_border_radius(ring_id + 1);
		if(inner_radius > tile.y){ continue; }

		const float dist                  = ring_center_radius(ring_id);
		const float rcp_intersection_deno = rcp(outer_radius - inner_radius);
		const float intersection_offset   = inner_radius * rcp_intersection_deno;

		const float theta_step = ring_angular_step(ring_id);
		const float theta_end    = 0.5 * PI - 0.25 * theta_step;
		[loop]
		for(float theta = ring_angular_offset(ring_id); theta < theta_end; theta += theta_step){
			const float2 offset = float2(cos(theta), sin(theta)) * dist;
			accumulateForeground(
				buckets, uv + texel_size * float2( offset.x,  offset.y),
				rcp_intersection_deno, intersection_offset, bucket_width);
			accumulateForeground(
				buckets, uv + texel_size * float2(-offset.y,  offset.x),
				rcp_intersection_deno, intersection_offset, bucket_width);
			accumulateForeground(
				buckets, uv + texel_size * float2(-offset.x, -offset.y),
				rcp_intersection_deno, intersection_offset, bucket_width);
			accumulateForeground(
				buckets, uv + texel_size * float2( offset.y, -offset.x),
				rcp_intersection_deno, intersection_offset, bucket_width);
		}
	}

	{	// Ring == 0
		const int   ring_id = 0;
		const float inner_radius = ring_border_radius(ring_id);
		const float outer_radius = ring_border_radius(ring_id + 1);

		const float dist                  = ring_center_radius(ring_id);
		const float rcp_intersection_deno = rcp(outer_radius - inner_radius);
		const float intersection_offset   = inner_radius * rcp_intersection_deno;

		accumulateForeground(
			buckets, uv, rcp_intersection_deno, intersection_offset, bucket_width);
	}

	float4 result = 0.0;
	{
		for(int i = 0; i < FOREGROUND_NUM_BUCKETS; ++i){
			const float w = buckets[i].weight;
			if(w > 0.0){
				result = alpha_blend(result, float4(buckets[i].color / w, saturate(w)));
			}
		}
	}

	const float alpha = saturate(result.a * FOREGROUND_ALPHA_MULTIPLE);
	return float4(result.rgb * alpha, alpha);
}

#endif
