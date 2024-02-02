//---------------------------------------------------------------------------
// Eye Selector (map step)
//
// Parameters:
// - BUFFER_WIDTH
// - BUFFER_HEIGHT
// - INPUT_SIZE
// - OUTPUT_SIZE
// - INPUT_TEXTURE
//---------------------------------------------------------------------------

#ifndef NN4VRC_YOLOX_EYE_SELECTOR_MAP_CGINC
#define NN4VRC_YOLOX_EYE_SELECTOR_MAP_CGINC

#include "UnityCG.cginc"
#include "../Common/Utility.cginc"
#include "DecodeBox.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

static const uint TASKS_PER_THREAD = ceil_div(INPUT_SIZE, OUTPUT_SIZE);

Texture2D<float4> INPUT_TEXTURE;


//---------------------------------------------------------------------------
// Vertex shader
//---------------------------------------------------------------------------

v2f vertex(appdata v){
	// Skip if face detection was failed
	const float4 face = INPUT_TEXTURE[int2(1, 0)];
	if(face.z * face.w == 0){
		v2f o;
		o.vertex = 0;
		o.uv     = 0;
		return o;
	}
	// Process normally
	const uint n_out = OUTPUT_SIZE * BUFFER_WIDTH * 8u;
	return vertex_impl(v, n_out);
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

Prediction load_input(uint index){
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

float compute_coverage(float4 f, float4 e){
	const float2 f0 = f.xy - f.zw * 0.5, f1 = f.xy + f.zw * 0.5;
	const float2 e0 = e.xy - e.zw * 0.5, e1 = e.xy + e.zw * 0.5;
	const float2 wh = max(0, min(f1, e1) - max(f0, e0));
	return (wh.x * wh.y) / (e.z * e.w);
}

float4 fragment(v2f i) : SV_TARGET {
	const uint raw_index = compute_raw_index(i);
	const uint step = OUTPUT_SIZE * BUFFER_WIDTH;

	const float4 face_box = INPUT_TEXTURE[int2(1, 0)];

	int   best_index = -1;
	float best_score = 1.#INF;
	for(uint k = 0; k < TASKS_PER_THREAD; ++k){
		const int index = raw_index + step * k;
		const Prediction cur = load_input(index);
		// Test eyeness
		if(cur.obj * cur.cls.y < CONF_THRESHOLD){ continue; }
		// Test unproject / depth
		if(cur.pos.w == 0){ continue; }
		// Test coverage
		// TODO use parameter?
		if(compute_coverage(face_box, cur.box) < 0.8){ continue; }

		// Select the nearest to camera
		const float score = cur.pos.z;
		if(score < best_score){
			best_index = index;
			best_score = score;
		}
	}
	return float4(best_score, best_index, 0, 0);
}


#endif
