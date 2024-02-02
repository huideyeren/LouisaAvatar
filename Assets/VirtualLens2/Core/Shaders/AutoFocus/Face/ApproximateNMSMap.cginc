//---------------------------------------------------------------------------
// Approximate NMS (map step)
//
// Parameters:
// - BUFFER_WIDTH
// - BUFFER_HEIGHT
// - INPUT_SIZE
// - IOU_THRESHOLD
// - INPUT_TEXTURE
//---------------------------------------------------------------------------

#ifndef NN4VRC_YOLOX_APPROXIMATE_NMS_MAP_CGINC
#define NN4VRC_YOLOX_APPROXIMATE_NMS_MAP_CGINC

#include "UnityCG.cginc"
#include "../Common/Utility.cginc"
#include "DecodeBox.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

static const uint LOG_INPUT_SIZE   = ceil_log2(INPUT_SIZE);
static const uint CEIL_INPUT_SIZE  = 1u << LOG_INPUT_SIZE;
static const uint THREADS_PER_BOX  = BUFFER_HEIGHT >> LOG_INPUT_SIZE;
static const uint TASKS_PER_THREAD = ceil_div(CEIL_INPUT_SIZE * BUFFER_WIDTH, THREADS_PER_BOX);

Texture2D<float4> INPUT_TEXTURE;


//---------------------------------------------------------------------------
// Vertex shader
//---------------------------------------------------------------------------

v2f vertex(appdata v){
	const uint n_out = BUFFER_WIDTH * BUFFER_HEIGHT * 8u;
	return vertex_impl(v, n_out);
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

struct Prediction {
	float4 raw;
	float4 box;
	float  obj;
	float2 cls;
};

Prediction load_input(uint index){
	const uint log_width = ceil_log2(BUFFER_WIDTH);
	const uint mask      = make_mask(log_width);
	const int2 coord     = int2(index & mask, index >> log_width);
	const float4 raw    = INPUT_TEXTURE[coord];
	const float  stride = get_decode_stride(coord);
	const float2 offset = get_decode_offset(coord);
	Prediction pred;
	pred.raw = raw;
	pred.box = float4(unpack_half2(raw.x) + offset, unpack_half2(raw.y)) * stride;
	pred.obj = unpack_half2(raw.z).x;
	pred.cls = unpack_half2(raw.w);
	return pred;
}

float confidence(Prediction pred){
	return pred.obj * max(pred.cls.x, pred.cls.y);
}

float area(float2 a){
	return a.x * a.y;
}

float compute_iou(float4 a, float4 b){
	const float2 a0 = a.xy - a.zw * 0.5, a1 = a.xy + a.zw * 0.5;
	const float2 b0 = b.xy - b.zw * 0.5, b1 = b.xy + b.zw * 0.5;
	const float i = area(max(0, min(a1, b1) - max(a0, b0)));
	const float u = area(a1 - a0) + area(b1 - b0) - i;
	return i / u;
}

float4 fragment(v2f i) : SV_TARGET {
	const uint raw_index = compute_raw_index(i);

	const uint i_width = LOG_INPUT_SIZE + ceil_log2(BUFFER_WIDTH);
	const uint i_mask  = make_mask(i_width);

	const uint out_i = raw_index & i_mask;
	const uint out_j = raw_index >> i_width;
	if(out_i >= INPUT_SIZE * BUFFER_WIDTH){ return 0; }

	const Prediction self = load_input(out_i);
	if(self.obj == 0){ return 0; }
	const float self_conf = confidence(self);

	bool accept = true;
	for(uint k = 0; k < TASKS_PER_THREAD; ++k){
		const uint index = out_j * TASKS_PER_THREAD + k;
		if(index >= INPUT_SIZE * BUFFER_WIDTH){ break; }
		const Prediction target = load_input(index);
		if((self.cls.x > self.cls.y) != (target.cls.x > target.cls.y)){ continue; }
		const float target_conf = confidence(target);
		if(target_conf <= self_conf){ continue; }
		if(compute_iou(self.box, target.box) < IOU_THRESHOLD){ continue; }
		accept = false;
	}

	if(!accept){ return 0; }
	return self.raw;
}


#endif
