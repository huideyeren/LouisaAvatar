//---------------------------------------------------------------------------
// Pixel Selector (last reduce step)
//
// Parameters:
// - BUFFER_WIDTH
// - BUFFER_HEIGHT
// - SOURCE_OFFSET
// - DESTINATION_OFFSET
// - INPUT_SIZE
// - INPUT_TEXTURE
//---------------------------------------------------------------------------

#ifndef NN4VRC_YOLOX_PIXEL_SELECTOR_REDUCE_LAST_CGINC
#define NN4VRC_YOLOX_PIXEL_SELECTOR_REDUCE_LAST_CGINC

#include "UnityCG.cginc"
#include "../Common/Utility.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

Texture2D<float4> INPUT_TEXTURE;


//---------------------------------------------------------------------------
// Vertex shader
//---------------------------------------------------------------------------

v2f vertex(appdata v){
	v2f o;

	if(!isComputeCamera(BUFFER_WIDTH, BUFFER_HEIGHT)){
		o.vertex = 0;
		o.uv     = 0;
		return o;
	}

	// Write to (8, 0)
	const float offset = 8.0 / BUFFER_WIDTH;
	const float width  = 1.0 / BUFFER_WIDTH;
	const float height = 1.0 / BUFFER_HEIGHT;

	const float x = v.uv.x * width + offset;
	const float y = v.uv.y * height;

	const float proj_x = (x * 2 - 1);
	const float proj_y = (y * 2 - 1) * _ProjectionParams.x;
	o.vertex = float4(proj_x, proj_y, 0, 1);
	o.uv     = v.uv;
	return o;
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

float4 load_input(uint index){
	const uint log_width = ceil_log2(BUFFER_WIDTH);
	const uint mask      = make_mask(log_width);
	const int2 coord     = int2(index & mask, (index >> log_width));
	return INPUT_TEXTURE[coord + int2(0, SOURCE_OFFSET)];
}

float4 fragment(v2f i) : SV_TARGET {
	const uint num_tasks = BUFFER_WIDTH * INPUT_SIZE;

	float4 best = float4(0, 0, 0, 1.#INF);
	for(uint k = 0; k < num_tasks; ++k){
		const float4 cur = load_input(k);
		if(cur.w < best.w){
			best = cur;
		}
	}
	if(isinf(best.w)){ return 0; }
	return float4(best.xyz, 1);
}


#endif
