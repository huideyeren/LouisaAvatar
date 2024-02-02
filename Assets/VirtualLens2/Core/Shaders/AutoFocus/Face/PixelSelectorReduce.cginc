//---------------------------------------------------------------------------
// Pixel Selector (reduce step)
//
// Parameters:
// - BUFFER_WIDTH
// - BUFFER_HEIGHT
// - SOURCE_OFFSET
// - DESTINATION_OFFSET
// - INPUT_SIZE
// - OUTPUT_SIZE
// - INPUT_TEXTURE
//---------------------------------------------------------------------------

#ifndef NN4VRC_YOLOX_PIXEL_SELECTOR_REDUCE_CGINC
#define NN4VRC_YOLOX_PIXEL_SELECTOR_REDUCE_CGINC

#include "UnityCG.cginc"
#include "../Common/Utility.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

static const uint TASKS_PER_THREAD = ceil_div(INPUT_SIZE, OUTPUT_SIZE);

Texture2D<float4> INPUT_TEXTURE;


//---------------------------------------------------------------------------
// Vertex shader
//---------------------------------------------------------------------------

v2f vertex(appdata v){
	const uint n_out = OUTPUT_SIZE * BUFFER_WIDTH * 8u;
	return vertex_impl(v, n_out);
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
	const uint raw_index = compute_raw_index(i);
	const uint step = OUTPUT_SIZE * BUFFER_WIDTH;

	float4 best = float4(0, 0, 0, 1.#INF);
	for(uint k = 0; k < TASKS_PER_THREAD; ++k){
		const float4 cur = load_input(raw_index + step * k);
		if(cur.w < best.w){
			best = cur;
		}
	}
	return best;
}


#endif
