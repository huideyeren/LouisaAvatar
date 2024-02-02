#ifndef VIRTUALLENS2_LOGIBOKEH_COMMON_BACKGROUND_BLUR_CGINC
#define VIRTUALLENS2_LOGIBOKEH_COMMON_BACKGROUND_BLUR_CGINC

#include "Samplers.cginc"
#include "Utility.cginc"


//---------------------------------------------------------------------------
// Structure definitions
//---------------------------------------------------------------------------

struct BackgroundBucket {
	float3 color;
	float  weight;
	float  coc_sum;
};


//---------------------------------------------------------------------------
// Utility functions
//---------------------------------------------------------------------------

float accumulateBackground(
	inout BackgroundBucket cur_bucket,
	inout BackgroundBucket prev_bucket,
	float2 coord,
	float  rcp_intersection_deno,
	float  intersection_offset,
	float  rcp_bucket_deno,
	float  bucket_offset)
{
	const float4 fetched      = fetchDownsampledBackground(linear_clamp_sampler, coord);
	const float  coc          = fetchBackgroundCocLinear(coord);
	const float3 rgb          = fetched.rgb;
	const float  mult         = fetched.a;
	const float  intersection = saturate(coc * rcp_intersection_deno - intersection_offset);
	const float  weight       = sample_alpha(coc) * intersection;
	const float  bucket       = saturate(coc * rcp_bucket_deno - bucket_offset);
	cur_bucket.color    += weight * (1.0 - bucket) * rgb;
	cur_bucket.weight   += weight * (1.0 - bucket) * mult;
	cur_bucket.coc_sum  += weight * (1.0 - bucket) * mult * coc;
	prev_bucket.color   += weight * bucket * rgb;
	prev_bucket.weight  += weight * bucket * mult;
	prev_bucket.coc_sum += weight * bucket * mult * coc;
	return saturate(weight * 1e3) * (1.0 - bucket);
}


float4 blurBackground(int num_rings, float2 uv){
	const float2 texel_size = DOWNSAMPLED_TEXEL_SIZE;
	const float2 tile = max(0.0, fetchDilatedTile(uv).xy);

	// Color only optimization
	if((tile.y - tile.x) < tile.y * 0.05){
		const float coc = (tile.x + tile.y) * 0.5;
		float4 acc = 0.0;
		[loop]
		for(int ring_id = num_rings - 1; ring_id > 0; --ring_id){
			const float inner_radius = ring_border_radius(ring_id);
			const float outer_radius = ring_border_radius(ring_id + 1);
			if(coc < inner_radius){ continue; }
			const float intersection = saturate((coc - inner_radius) * rcp(outer_radius - inner_radius));
			const float weight       = sample_alpha(coc) * intersection;
			const float dist         = ring_center_radius(ring_id);
			const float theta_step   = ring_angular_step(ring_id);
			const float theta_end    = 0.5 * PI - 0.25 * theta_step;
			[loop]
			for(float theta = ring_angular_offset(ring_id); theta < theta_end; theta += theta_step){
				const float2 offset = float2(cos(theta), sin(theta)) * dist;
				const float2 coord0 = uv + texel_size * float2( offset.x,  offset.y);
				const float2 coord1 = uv + texel_size * float2(-offset.y,  offset.x);
				const float2 coord2 = uv + texel_size * float2(-offset.x, -offset.y);
				const float2 coord3 = uv + texel_size * float2( offset.y, -offset.x);
				acc += fetchDownsampledBackground(linear_clamp_sampler, coord0) * weight;
				acc += fetchDownsampledBackground(linear_clamp_sampler, coord1) * weight;
				acc += fetchDownsampledBackground(linear_clamp_sampler, coord2) * weight;
				acc += fetchDownsampledBackground(linear_clamp_sampler, coord3) * weight;
			}
		}
		{	// ring_id == 0
			const float outer_radius = ring_border_radius(1);
			const float intersection = saturate(coc * rcp(outer_radius));
			const float weight       = sample_alpha(coc) * intersection;
			acc += fetchDownsampledBackground(linear_clamp_sampler, uv) * weight;
		}
		return acc;
	}

	// Scatter-as-gather with descending ring bucketing
	BackgroundBucket prev_bucket;
	prev_bucket.color   = 0.0;
	prev_bucket.weight  = 0.0;
	prev_bucket.coc_sum = 0.0;

	[loop]
	for(int ring_id = num_rings - 1; ring_id > 0; --ring_id){
		const float inner_radius = ring_border_radius(ring_id);
		const float outer_radius = ring_border_radius(ring_id + 1);
		if(inner_radius > tile.y){ continue; }

		const float border_min = ring_border_radius(ring_id + 1);
		const float border_max = ring_border_radius(ring_id + 2);
		const float dist       = ring_center_radius(ring_id);

		const float rcp_intersection_deno = rcp(outer_radius - inner_radius);
		const float intersection_offset   = inner_radius * rcp_intersection_deno;

		const float rcp_bucket_deno = rcp(border_max - border_min);
		const float bucket_offset   = border_min * rcp_bucket_deno;

		BackgroundBucket cur_bucket;
		cur_bucket.color   = 0.0;
		cur_bucket.weight  = 0.0;
		cur_bucket.coc_sum = 0.0;
		float cur_count    = 0.0;

		const float theta_step = ring_angular_step(ring_id);
		const float theta_end  = 0.5 * PI - 0.25 * theta_step;
		[loop]
		for(float theta = ring_angular_offset(ring_id); theta < theta_end; theta += theta_step){
			const float2 offset = float2(cos(theta), sin(theta)) * dist;
			cur_count += accumulateBackground(
				cur_bucket, prev_bucket,
				uv + texel_size * float2( offset.x,  offset.y),
				rcp_intersection_deno, intersection_offset,
				rcp_bucket_deno, bucket_offset);
			cur_count += accumulateBackground(
				cur_bucket, prev_bucket,
				uv + texel_size * float2(-offset.y,  offset.x),
				rcp_intersection_deno, intersection_offset,
				rcp_bucket_deno, bucket_offset);
			cur_count += accumulateBackground(
				cur_bucket, prev_bucket,
				uv + texel_size * float2(-offset.x, -offset.y),
				rcp_intersection_deno, intersection_offset,
				rcp_bucket_deno, bucket_offset);
			cur_count += accumulateBackground(
				cur_bucket, prev_bucket,
				uv + texel_size * float2( offset.y, -offset.x),
				rcp_intersection_deno, intersection_offset,
				rcp_bucket_deno, bucket_offset);
		}

		if(prev_bucket.weight == 0.0){
			prev_bucket = cur_bucket;
		}else if(cur_count > 0.0){
			const float prev_coc_avg  = prev_bucket.coc_sum / prev_bucket.weight;
			const float cur_coc_avg   = cur_bucket.coc_sum  / cur_bucket.weight;
			const float opacity       = cur_count / ring_density(ring_id);
			const float occluding_coc = saturate(prev_coc_avg - cur_coc_avg);
			const float alpha         = 1.0 - opacity * occluding_coc;
			prev_bucket.color   = prev_bucket.color   * alpha + cur_bucket.color;
			prev_bucket.weight  = prev_bucket.weight  * alpha + cur_bucket.weight;
			prev_bucket.coc_sum = prev_bucket.coc_sum * alpha + cur_bucket.coc_sum;
		}
	}

	{	// Ring == 0
		const int   ring_id = 0;
		const float inner_radius = ring_border_radius(ring_id);
		const float outer_radius = ring_border_radius(ring_id + 1);

		const float border_min = ring_border_radius(ring_id + 1);
		const float border_max = ring_border_radius(ring_id + 2);
		const float dist       = ring_center_radius(ring_id);

		const float rcp_intersection_deno = rcp(outer_radius - inner_radius);
		const float intersection_offset   = inner_radius * rcp_intersection_deno;

		const float rcp_bucket_deno = rcp(border_max - border_min);
		const float bucket_offset   = border_min * rcp_bucket_deno;

		BackgroundBucket cur_bucket;
		cur_bucket.color   = 0.0;
		cur_bucket.weight  = 0.0;
		cur_bucket.coc_sum = 0.0;
		float cur_count    = 0.0;

		cur_count = accumulateBackground(
			cur_bucket, prev_bucket, uv,
			rcp_intersection_deno, intersection_offset,
			rcp_bucket_deno, bucket_offset);

		if(prev_bucket.weight == 0.0){
			prev_bucket = cur_bucket;
		}else if(cur_count > 0.0){
			const float prev_coc_avg  = prev_bucket.coc_sum / prev_bucket.weight;
			const float cur_coc_avg   = cur_bucket.coc_sum  / cur_bucket.weight;
			const float opacity       = cur_count / ring_density(ring_id);
			const float occluding_coc = saturate(prev_coc_avg - cur_coc_avg);
			const float alpha         = 1.0 - opacity * occluding_coc;
			prev_bucket.color   = prev_bucket.color   * alpha + cur_bucket.color;
			prev_bucket.weight  = prev_bucket.weight  * alpha + cur_bucket.weight;
			prev_bucket.coc_sum = prev_bucket.coc_sum * alpha + cur_bucket.coc_sum;
		}
	}

	if(prev_bucket.weight == 0.0){
		return float4(0, 0, 0, 0);
	}else{
		return float4(prev_bucket.color, prev_bucket.weight);
	}
}

#endif
