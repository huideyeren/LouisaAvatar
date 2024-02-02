//---------------------------------------------------------------------------
// Face Selector (map step)
//
// Parameters:
// - BUFFER_WIDTH
// - BUFFER_HEIGHT
// - SOURCE_BOX_OFFSET
// - SOURCE_POINT_OFFSET
// - DESTINATION_OFFSET
// - INPUT_SIZE
// - OUTPUT_SIZE
// - INPUT_WIDTH
// - INPUT_HEIGHT
// - STRIDE
// - INPUT_TEXTURE
//---------------------------------------------------------------------------

#ifndef NN4VRC_YOLOX_FACE_SELECTOR_MAP_CGINC
#define NN4VRC_YOLOX_FACE_SELECTOR_MAP_CGINC

#include "UnityCG.cginc"
#include "../Common/Utility.cginc"
#include "../../Common/StateTexture.cginc"
#include "../../Common/TouchTexture.cginc"
#include "../../Common/CameraPoseTexture.cginc"
#include "DecodeBox.cginc"
#include "TrackingParameters.cginc"


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

float2 uv2coord(float2 uv){
	const float2 scale = float2(IMAGE_WIDTH, IMAGE_HEIGHT);
	const float2 offset = float2(
		(IMAGE_WIDTH  - INPUT_WIDTH  * STRIDE) * 0.5,
		(IMAGE_HEIGHT - INPUT_HEIGHT * STRIDE) * 0.5);
	return float2(uv.x, 1 - uv.y) * scale - offset;
}

float4 fragment(v2f i) : SV_TARGET {
	const uint raw_index = compute_raw_index(i);
	const uint step = OUTPUT_SIZE * BUFFER_WIDTH;

	const bool is_touching = isTouching();
	const float2 touching_point = uv2coord(getTouchingPoint());

	const float4x4 v2w = getInversedViewMatrix();
	const float3 last_focus = getFocusPointInWorld();

	int   best_index = -1;
	float best_score = 1.#INF;
	for(uint k = 0; k < TASKS_PER_THREAD; ++k){
		const int index = raw_index + step * k;
		const Prediction cur = load_input(index);
		// Test faceness
		if(cur.obj * cur.cls.x < CONF_THRESHOLD){ continue; }
		// Test unproject / depth
		if(cur.pos.w == 0){ continue; }

		float score = 1.#INF;
		if(is_touching){
			// Nearest to touching point in screen space
			const float2 d = touching_point - cur.box.xy;
			score = length(d);
		}else{
			// Nearest to previous point in world space
			const float4 p = mul(v2w, cur.pos);
			const float s = length(p.xyz - last_focus);
			if(s < get_face_tracking_distance_limit()){
				score = s;
			}
		}
		// TODO? timeout => nearest box
		if(score < best_score){
			best_index = index;
			best_score = score;
		}
	}
	return float4(best_score, best_index, 0, 0);
}


#endif
