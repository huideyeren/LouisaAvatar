#ifndef VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_HIGH_RESOLUTION_TILE_CONFIG_CGINC
#define VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_HIGH_RESOLUTION_TILE_CONFIG_CGINC

#include "../Common/Samplers.cginc"

/* _TileTex layout
 *
 *   +------------+------------+
 *   |     RT     | RT Dilated |
 *   +------------+------------+
 *   |                         |
 *   |       HR Dilated        |
 *   |                         |
 *   +-------------------------+
 *   |                         |
 *   |            HR           |
 *   |                         |
 *   +-------------------------+
 *
 *  - HR: 256 x 144 
 *  - RT: 128 x  72
 */

Texture2D _TileTex;

float4 fetchTile(float2 uv){
	const float2 minval = float2(0.0, 0.0);
	const float2 maxval = float2(1.0, 2.0 / 5.0);
	const float2 scale  = maxval - minval;
	const float2 offset = minval;
	return _TileTex.Sample(point_clamp_sampler, uv * scale + offset);
}

float4 fetchDilatedTile(float2 uv){
	const float2 minval = float2(0.0, 2.0 / 5.0);
	const float2 maxval = float2(1.0, 4.0 / 5.0);
	const float2 scale  = maxval - minval;
	const float2 offset = minval;
	return _TileTex.Sample(point_clamp_sampler, uv * scale + offset);
}

#endif
