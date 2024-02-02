//---------------------------------------------------------------------------
// Prediction
//
// Parameters:
// - BUFFER_WIDTH
// - BUFFER_HEIGHT
// - REG_SOURCE_OFFSET
// - CLS_SOURCE_OFFSET
// - DESTINATION_OFFSET
// - INPUT_WIDTH
// - INPUT_HEIGHT
// - INPUT_CHANNELS
// - CONF_THRESHOLD
// - INPUT_TEXTURE
// - PARAMETER_TEXTURE
// - WEIGHT_OFFSET_X
// - WEIGHT_OFFSET_Y
// - BIAS_OFFSET_X
// - BIAS_OFFSET_Y
// - SKIP_DECODE
//---------------------------------------------------------------------------

#ifndef NN4VRC_YOLOX_PREDICTION_CGINC
#define NN4VRC_YOLOX_PREDICTION_CGINC

#include "UnityCG.cginc"
#include "../Common/Utility.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

static const uint OUTPUT_WIDTH  = INPUT_WIDTH;
static const uint OUTPUT_HEIGHT = INPUT_HEIGHT;

static const uint LOG_INPUT_WIDTH   = ceil_log2(INPUT_WIDTH);
static const uint LOG_INPUT_HEIGHT  = ceil_log2(INPUT_HEIGHT);
static const uint LOG_OUTPUT_WIDTH  = ceil_log2(OUTPUT_WIDTH);
static const uint LOG_OUTPUT_HEIGHT = ceil_log2(OUTPUT_HEIGHT);

static const uint CEIL_INPUT_WIDTH   = 1u << LOG_INPUT_WIDTH;
static const uint CEIL_INPUT_HEIGHT  = 1u << LOG_INPUT_HEIGHT;
static const uint CEIL_OUTPUT_WIDTH  = 1u << LOG_OUTPUT_WIDTH;
static const uint CEIL_OUTPUT_HEIGHT = 1u << LOG_OUTPUT_HEIGHT;

Texture2D<float4> INPUT_TEXTURE;
Texture2D<float4> PARAMETER_TEXTURE;


//---------------------------------------------------------------------------
// Vertex shader
//---------------------------------------------------------------------------

v2f vertex(appdata v){
	const uint n_out = CEIL_OUTPUT_WIDTH * CEIL_OUTPUT_HEIGHT * 8u;
	return vertex_impl(v, n_out);
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

float4 load_reg_input(uint index){
	const uint log_width = ceil_log2(BUFFER_WIDTH);
	const uint mask      = make_mask(log_width);
	return INPUT_TEXTURE[int2(index & mask, (index >> log_width) + REG_SOURCE_OFFSET)];
}

float4 load_cls_input(uint index){
	const uint log_width = ceil_log2(BUFFER_WIDTH);
	const uint mask      = make_mask(log_width);
	return INPUT_TEXTURE[int2(index & mask, (index >> log_width) + CLS_SOURCE_OFFSET)];
}

float4 load_weight(uint c){
	return PARAMETER_TEXTURE[int2(c + WEIGHT_OFFSET_X, WEIGHT_OFFSET_Y)];
}

float4 load_bias(){
	return PARAMETER_TEXTURE[int2(BIAS_OFFSET_X, BIAS_OFFSET_Y)];
}

float4 fragment(v2f i) : SV_TARGET {
	const uint raw_index = compute_raw_index(i);

	const uint x0_width = LOG_OUTPUT_WIDTH  - 1u;
	const uint y0_width = LOG_OUTPUT_HEIGHT - 1u;
	const uint x0_mask = make_mask(x0_width);
	const uint y0_mask = make_mask(y0_width);

	const uint out_x0 = raw_index & x0_mask;
	const uint out_y0 = (raw_index >> x0_width) & y0_mask;
	const uint out_z  = raw_index >> (x0_width + y0_width);
	const uint out_x  = out_x0 * 2u + (out_z &  1);
	const uint out_y  = out_y0 * 2u + (out_z >> 1);
	if(out_x >= OUTPUT_WIDTH || out_y >= OUTPUT_HEIGHT){ return 0; }

	const uint x_step = 1u;
	const uint y_step = (CEIL_INPUT_WIDTH  / 2u) * x_step;
	const uint c_step = (CEIL_INPUT_HEIGHT / 2u) * y_step;
	const uint index0 =
		  out_x0 * x_step
		+ out_y0 * y_step;

	const float4 bias = load_bias();
	float4 acc_box = float4(unpack_half2(bias.x), unpack_half2(bias.y));
	float  acc_obj = unpack_half2(bias.z).x;
	float2 acc_cls = unpack_half2(bias.w);

#define ACCUMULATE(suffix) \
	[unroll] \
	do { \
		for(uint c0 = 0; c0 < ceil_div(INPUT_CHANNELS, 2u); ++c0){ \
			const float4 w0 = load_weight(c0 * 2u + 0u); \
			const float4 w1 = load_weight(c0 * 2u + 1u); \
			const float4 w0_box = float4(unpack_half2(w0.x), unpack_half2(w0.y)); \
			const float4 w1_box = float4(unpack_half2(w1.x), unpack_half2(w1.y)); \
			const float  w0_obj = unpack_half2(w0.z).x; \
			const float  w1_obj = unpack_half2(w1.z).x; \
			const float2 w0_cls = unpack_half2(w0.w); \
			const float2 w1_cls = unpack_half2(w1.w); \
			const float2 in_reg = unpack_half2(load_reg_input(index0 + c0 * c_step).suffix); \
			const float2 in_cls = unpack_half2(load_cls_input(index0 + c0 * c_step).suffix); \
			acc_box += w0_box * in_reg.x; \
			acc_obj += w0_obj * in_reg.x; \
			acc_cls += w0_cls * in_cls.x; \
			acc_box += w1_box * in_reg.y; \
			acc_obj += w1_obj * in_reg.y; \
			acc_cls += w1_cls * in_cls.y; \
		} \
	} while(false)

	if(out_z == 0){
		ACCUMULATE(x);
	}else if(out_z == 1){
		ACCUMULATE(y);
	}else if(out_z == 2){
		ACCUMULATE(z);
	}else if(out_z == 3){
		ACCUMULATE(w);
	}

#undef ACCUMULATE

	acc_obj = 1.0 / (1.0 + exp(-acc_obj));
	acc_cls = 1.0 / (1.0 + exp(-acc_cls));

#ifndef SKIP_DECODE
	const float cls_max = max(acc_cls.x, acc_cls.y);
	if(acc_obj * cls_max < CONF_THRESHOLD){
		acc_box = 0;
		acc_obj = 0;
		acc_cls = 0;
	}else{
		acc_box.x = acc_box.x;
		acc_box.y = acc_box.y;
		acc_box.z = exp(acc_box.z);
		acc_box.w = exp(acc_box.w);
	}
#endif

	return float4(
		pack_half2(acc_box.xy),
		pack_half2(acc_box.zw),
		pack_half2(float2(acc_obj, 1)),
		pack_half2(acc_cls));
}


#endif
