#ifndef VIRTUALLENS2_LOGIBOKEH_COMMON_TILE_DILATOR_CGINC
#define VIRTUALLENS2_LOGIBOKEH_COMMON_TILE_DILATOR_CGINC

//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

Texture2D _GrabTexture;
float4 _GrabTexture_TexelSize;


//---------------------------------------------------------------------------
// Utility functions
//---------------------------------------------------------------------------

float4 dilateTile(int num_rings, float2 uv, float2 region_min, float2 region_max){
	const float2 texel_size = abs(_GrabTexture_TexelSize.xy);
	const float2 clamp_min = region_min + 0.5 * texel_size;
	const float2 clamp_max = region_max - 0.5 * texel_size;
	const float2 center    = uv * (region_max - region_min) + region_min;
	const int radius = (int)ceil(ring_border_radius(num_rings) / 8.0);
	float4 agg = _GrabTexture.Sample(point_clamp_sampler, center);
	{
		for(int y = -radius; y <= radius; ++y){
			for(int x = -radius; x <= radius; ++x){
				const float  dy = 8.0 * max(abs(y) - 1, 0);
				const float  dx = 8.0 * max(abs(x) - 1, 0);
				const float  d  = length(float2(dx, dy));
				const float4 v  = _GrabTexture.Sample(
					point_clamp_sampler, clamp(center + texel_size * float2(x, y), clamp_min, clamp_max));
				if(v.y >= d){ agg.y = max(agg.y, v.y); }
				if(v.w >= d){ agg.w = max(agg.w, v.w); }
			}
		}
	}
	{
		for(int y = -radius; y <= radius; ++y){
			for(int x = -radius; x <= radius; ++x){
				const float  dy = 8.0 * max(abs(y) - 1, 0);
				const float  dx = 8.0 * max(abs(x) - 1, 0);
				const float  d  = length(float2(dx, dy));
				const float4 v  = _GrabTexture.Sample(
					point_clamp_sampler, clamp(center + texel_size * float2(x, y), clamp_min, clamp_max));
				if(agg.y >= d){ agg.x = min(agg.x, v.x); }
				if(agg.w >= d){ agg.z = min(agg.z, v.z); }
			}
		}
	}
	return agg;
}

#endif
