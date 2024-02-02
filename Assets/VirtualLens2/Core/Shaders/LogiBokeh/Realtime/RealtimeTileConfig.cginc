#ifndef VIRTUALLENS2_LOGIBOKEH_REALTIME_REALTIME_TILE_CONFIG_CGINC
#define VIRTUALLENS2_LOGIBOKEH_REALTIME_REALTIME_TILE_CONFIG_CGINC

#include "../Common/Samplers.cginc"

/* _TileTex layout
 *
 *   +-------------+
 *   |             |
 *   |  RT Dilate  |
 *   |             |
 *   +-------------+
 *   |             |
 *   |     RT      |
 *   |             |
 *   +-------------+
 *
 *  - RT: 128 x  72 (Dilated)
 *  - RT: 128 x  72
 */

Texture2D _TileTex;

float4 fetchTile(float2 uv){
	uv = uv * float2(1.0, 0.5) + float2(0.0, 0.0);
	return _TileTex.Sample(point_clamp_sampler, uv);
}

float4 fetchDilatedTile(float2 uv){
	uv = uv * float2(1.0, 0.5) + float2(0.0, 0.5);
	return _TileTex.Sample(point_clamp_sampler, uv);
}

#endif
