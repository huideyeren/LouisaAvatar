//---------------------------------------------------------------------------
// Approximate NMS (reduce step)
//
// Parameters:
// - BUFFER_WIDTH
// - BUFFER_HEIGHT
// - INPUT_SIZE
// - INPUT_TEXTURE
//---------------------------------------------------------------------------

#ifndef NN4VRC_YOLOX_APPROXIMATE_NMS_MAP_CGINC
#define NN4VRC_YOLOX_APPROXIMATE_NMS_MAP_CGINC

#include "UnityCG.cginc"
#include "../Common/Utility.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

static const uint LOG_INPUT_SIZE   = ceil_log2(INPUT_SIZE);
static const uint CEIL_INPUT_SIZE  = 1u << LOG_INPUT_SIZE;
static const uint THREADS_PER_BOX  = BUFFER_HEIGHT >> LOG_INPUT_SIZE;

Texture2D<float4> INPUT_TEXTURE;


//---------------------------------------------------------------------------
// Vertex shader
//---------------------------------------------------------------------------

v2f vertex(appdata v){
	const uint n_out = BUFFER_WIDTH * INPUT_SIZE * 8u;
	return vertex_impl(v, n_out);
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

float4 load_input(uint index){
	const uint log_width = ceil_log2(BUFFER_WIDTH);
	const uint mask      = make_mask(log_width);
	return INPUT_TEXTURE[int2(index & mask, index >> log_width)];
}

bool is_accepted(float4 v){
	return unpack_half2(v.z).y != 0;
}

float4 fragment(v2f i) : SV_TARGET {
	const uint index = compute_raw_index(i);
	const uint step  = CEIL_INPUT_SIZE * BUFFER_WIDTH;

	const float4 self = load_input(index);

	bool accept = is_accepted(self);
	for(uint k = 1; k < THREADS_PER_BOX; ++k){
		const float4 v = load_input(index + k * step);
		if(!is_accepted(v)){ accept = false; }
	}

	if(!accept){
		return 0;
	}else{
		return self;
	}
}


#endif
