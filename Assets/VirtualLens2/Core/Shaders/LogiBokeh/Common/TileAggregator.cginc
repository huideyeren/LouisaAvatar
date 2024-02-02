#ifndef VIRTUALLENS2_LOGIBOKEH_COMMON_TILE_AGGREGATOR_CGINC
#define VIRTUALLENS2_LOGIBOKEH_COMMON_TILE_AGGREGATOR_CGINC

//---------------------------------------------------------------------------
// Utility functions
//---------------------------------------------------------------------------

float4 aggregateCocSample(float4 cur, float2 uv){
	const float4 bg = gatherBackgroundCoc(uv);
	const float4 fg = gatherForegroundCoc(uv);
	return float4(
		min(cur.x, min(min(bg.x, bg.y), min(bg.z, bg.w))),
		max(cur.y, max(max(bg.x, bg.y), max(bg.z, bg.w))),
		min(cur.z, min(min(fg.x, fg.y), min(fg.z, fg.w))),
		max(cur.w, max(max(fg.x, fg.y), max(fg.z, fg.w))));
}

float4 aggregateTile(float2 uv){
	const float2 texel_size = HALF_COC_TEXEL_SIZE;
	float4 agg = float4(1e3, -1e3, 1e3, -1e3);
	agg = aggregateCocSample(agg, uv + texel_size * float2(-3, -3));
	agg = aggregateCocSample(agg, uv + texel_size * float2(-1, -3));
	agg = aggregateCocSample(agg, uv + texel_size * float2( 1, -3));
	agg = aggregateCocSample(agg, uv + texel_size * float2( 3, -3));
	agg = aggregateCocSample(agg, uv + texel_size * float2(-3, -1));
	agg = aggregateCocSample(agg, uv + texel_size * float2(-1, -1));
	agg = aggregateCocSample(agg, uv + texel_size * float2( 1, -1));
	agg = aggregateCocSample(agg, uv + texel_size * float2( 3, -1));
	agg = aggregateCocSample(agg, uv + texel_size * float2(-3,  1));
	agg = aggregateCocSample(agg, uv + texel_size * float2(-1,  1));
	agg = aggregateCocSample(agg, uv + texel_size * float2( 1,  1));
	agg = aggregateCocSample(agg, uv + texel_size * float2( 3,  1));
	agg = aggregateCocSample(agg, uv + texel_size * float2(-3,  3));
	agg = aggregateCocSample(agg, uv + texel_size * float2(-1,  3));
	agg = aggregateCocSample(agg, uv + texel_size * float2( 1,  3));
	agg = aggregateCocSample(agg, uv + texel_size * float2( 3,  3));
	return agg;
}

#endif
