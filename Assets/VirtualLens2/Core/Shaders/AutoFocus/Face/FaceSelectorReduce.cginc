//---------------------------------------------------------------------------
// Face Selector (reduce step)
//
// Parameters:
// - BUFFER_WIDTH
// - BUFFER_HEIGHT
// - INPUT_SIZE
// - INPUT_WIDTH
// - INPUT_HEIGHT
// - OUTPUT_WIDTH
// - OUTPUT_HEIGHT
// - INPUT_TEXTURE
//---------------------------------------------------------------------------

#ifndef NN4VRC_YOLOX_FACE_SELECTOR_REDUCE_CGINC
#define NN4VRC_YOLOX_FACE_SELECTOR_REDUCE_CGINC

#include "UnityCG.cginc"
#include "../Common/Utility.cginc"
#include "DecodeBox.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

static const uint OFFSET_X = (IMAGE_WIDTH  - INPUT_WIDTH  * STRIDE)  / 2;
static const uint OFFSET_Y = (IMAGE_HEIGHT - INPUT_HEIGHT * STRIDE) / 2;

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

	// Write to (0, 0) and (3, 0)
	const float width  = 4.0 / BUFFER_WIDTH;
	const float height = 1.0 / BUFFER_HEIGHT;

	const float x = v.uv.x * width;
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

struct Prediction {
	float4 box;
	float  obj;
	float2 cls;
	float4 pos;
};

float4 load_input(uint index){
	const uint log_width = ceil_log2(BUFFER_WIDTH);
	const uint mask      = make_mask(log_width);
	const int2 coord = int2(index & mask, (index >> log_width) + SOURCE_OFFSET);
	const float4 raw = INPUT_TEXTURE[coord];
	return raw;
}

Prediction load_prediction(uint index){
	const uint log_width = ceil_log2(BUFFER_WIDTH);
	const uint mask      = make_mask(log_width);
	const int2 coord     = int2(index & mask, (index >> log_width));
	const float4 raw    = INPUT_TEXTURE[coord + int2(0, SOURCE_BOX_OFFSET)];
	const float  stride = get_decode_stride(coord);
	const float2 offset = get_decode_offset(coord);
	Prediction pred;
	pred.box = float4(unpack_half2(raw.x) + offset, unpack_half2(raw.y)) * stride;
	pred.obj = unpack_half2(raw.z).x;
	pred.cls = unpack_half2(raw.w);
	pred.pos = INPUT_TEXTURE[coord + int2(0, SOURCE_POINT_OFFSET)];
	return pred;
}

float4 fragment(v2f i) : SV_TARGET {
	const uint num_tasks = BUFFER_WIDTH * INPUT_SIZE;

	float4 best_box   = 0;
	float  best_score = 1.#INF;
	float4 best_pos   = 0;
	for(uint k = 0; k < num_tasks; ++k){
		const float4 t = load_input(k);
		const float score = t.x;
		const int   index = (int)t.y;
		if(index < 0 || score >= best_score){ continue; }
		const Prediction pred = load_prediction(index);
		best_box   = pred.box;
		best_score = score;
		best_pos   = pred.pos;
	}
	if(isinf(best_score)){ return 0; }

	if(i.uv.x < 0.25){
		// Write bounding box in uv space
		return float4(
			(best_box.x + OFFSET_X) / IMAGE_WIDTH,
			1.0 - (best_box.y + OFFSET_Y) / IMAGE_HEIGHT,
			best_box.z / IMAGE_WIDTH,
			best_box.w / IMAGE_HEIGHT);
	}else if(i.uv.x < 0.5){
		// Write bounding box in image space
		return best_box;
	}else if(i.uv.x < 0.75){
		// Write focus point in view space
		return best_pos;
	}else{
		return 0;
	}
}


#endif
