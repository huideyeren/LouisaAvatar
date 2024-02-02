//---------------------------------------------------------------------------
// Unproject (conversion from projected space to view space)
//
// Parameters:
// - BUFFER_WIDTH
// - BUFFER_HEIGHT
// - SOURCE_OFFSET
// - DESTINATION_OFFSET
// - INPUT_SIZE
// - INPUT_WIDTH
// - INPUT_HEIGHT
// - STRIDE
// - IMAGE_WIDTH
// - IMAGE_HEIGHT
// - INPUT_TEXTURE
// - DEPTH_TEXTURE
//---------------------------------------------------------------------------

#ifndef NN4VRC_YOLOX_UNPROJECT_CGINC
#define NN4VRC_YOLOX_UNPROJECT_CGINC

#include "UnityCG.cginc"
#include "../Common/Utility.cginc"
#include "../../Common/Constants.cginc"
#include "DecodeBox.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

float _Near;
float _Far;
float _FieldOfView;

static const uint OUTPUT_SIZE = INPUT_SIZE;

Texture2D<float4> INPUT_TEXTURE;
Texture2D<float>  DEPTH_TEXTURE;

SamplerState point_clamp_sampler;


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
};

Prediction load_input(uint index){
	const uint log_width = ceil_log2(BUFFER_WIDTH);
	const uint mask      = make_mask(log_width);
	const int2 coord     = int2(index & mask, (index >> log_width));
	const float4 raw    = INPUT_TEXTURE[coord + int2(0, SOURCE_OFFSET)];
	const float  stride = get_decode_stride(coord);
	const float2 offset = get_decode_offset(coord);
	Prediction pred;
	pred.box = float4(unpack_half2(raw.x) + offset, unpack_half2(raw.y)) * stride;
	pred.obj = unpack_half2(raw.z).x;
	pred.cls = unpack_half2(raw.w);
	return pred;
}

float2 coord2uv(float2 uv){
	const float2 scale = 1.0 / float2(IMAGE_WIDTH, IMAGE_HEIGHT);
	const float2 offset = float2(
		(IMAGE_WIDTH  - INPUT_WIDTH  * STRIDE) * 0.5,
		(IMAGE_HEIGHT - INPUT_HEIGHT * STRIDE) * 0.5);
	uv = (uv + offset) * scale;
	return float2(uv.x, 1.0 - uv.y);
}

float load_depth(float2 uv){
	return DEPTH_TEXTURE.Sample(point_clamp_sampler, uv);
}

float linearize_depth(float d){
	const float z = 1.0 / _Far - 1.0 / _Near;
	const float w = 1.0 / _Near;
	return 1.0 / (z * (1.0 - d) + w);
}

float4 fragment(v2f i) : SV_TARGET {
	const uint raw_index = compute_raw_index(i);
	const Prediction pred = load_input(raw_index);
	
	// Ignore if box has no object
	if(pred.obj * max(pred.cls.x, pred.cls.y) < CONF_THRESHOLD){ return 0; }

	// Compute median depth from 9 samples
	const float2 uvs[3] = {
		coord2uv(pred.box.xy - 0.25 * pred.box.zw),
		coord2uv(pred.box.xy),
		coord2uv(pred.box.xy + 0.25 * pred.box.zw)
	};
	float depths[6] = { 0, 0, 0, 0, 0, 0 };

	[unroll]
	for(int i = 0; i < 3; ++i){
		[unroll]
		for(int j = 0; j < 3; ++j){
			depths[5] = load_depth(float2(uvs[j].x, uvs[i].y));
			[unroll]
			for(int k = 4; k >= 0; --k){
				if(depths[k + 1] > depths[k]){
					const float t = depths[k];
					depths[k] = depths[k + 1];
					depths[k + 1] = t;
				}else{
					break;
				}
			}
		}
	}

	const float depth = depths[4];
	if(depth == 0){ return 0; }

	// Unproject point
	const float DEG2RAD = 0.01745329251;
	const float3 projected = float3(uvs[1] * 2.0 - 1.0, depth);
	const float z = linearize_depth(projected.z);
	const float y_scale = tan((PI - _FieldOfView * DEG2RAD) * 0.5);
	const float x_scale = y_scale / ASPECT;
	const float y = projected.y * z / y_scale;
	const float x = projected.x * z / x_scale;
	return float4(x, y, z, 1);
}


#endif
