//---------------------------------------------------------------------------
// DepthwiseConvolution3x3 (pad=1)
//
// Parameters:
// - BUFFER_WIDTH
// - BUFFER_HEIGHT
// - SOURCE_OFFSET
// - DESTINATION_OFFSET
// - INPUT_WIDTH
// - INPUT_HEIGHT
// - INPUT_CHANNELS
// - INPUT_TEXTURE
// - WEIGHT_TEXTURE
// - BIAS_TEXTURE
// - WEIGHT_OFFSET_X
// - WEIGHT_OFFSET_Y
// - BIAS_OFFSET_X
// - BIAS_OFFSET_Y
//---------------------------------------------------------------------------

#ifndef NN4VRC_COMMON_DEPTHWISE_CONVOLUTION_3x3s2p1_CGINC
#define NN4VRC_COMMON_DEPTHWISE_CONVOLUTION_3x3s2p1_CGINC

#include "UnityCG.cginc"
#include "Utility.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

static const uint OUTPUT_WIDTH    = INPUT_WIDTH;
static const uint OUTPUT_HEIGHT   = INPUT_HEIGHT;
static const uint OUTPUT_CHANNELS = INPUT_CHANNELS;

static const uint LOG_INPUT_WIDTH   = ceil_log2(INPUT_WIDTH);
static const uint LOG_INPUT_HEIGHT  = ceil_log2(INPUT_HEIGHT);
static const uint LOG_OUTPUT_WIDTH  = ceil_log2(OUTPUT_WIDTH);
static const uint LOG_OUTPUT_HEIGHT = ceil_log2(OUTPUT_HEIGHT);

static const uint CEIL_INPUT_WIDTH   = 1u << LOG_INPUT_WIDTH;
static const uint CEIL_INPUT_HEIGHT  = 1u << LOG_INPUT_HEIGHT;
static const uint CEIL_OUTPUT_WIDTH  = 1u << LOG_OUTPUT_WIDTH;
static const uint CEIL_OUTPUT_HEIGHT = 1u << LOG_OUTPUT_HEIGHT;

Texture2D<float4> INPUT_TEXTURE;
Texture2D<float4> WEIGHT_TEXTURE;
Texture2D<float2> BIAS_TEXTURE;


//---------------------------------------------------------------------------
// Vertex shader
//---------------------------------------------------------------------------

v2f vertex(appdata v){
	const uint n_out = CEIL_OUTPUT_WIDTH * CEIL_OUTPUT_HEIGHT * OUTPUT_CHANNELS;
	return vertex_impl(v, n_out);
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

float4 load_input(uint index){
	const uint log_width = ceil_log2(BUFFER_WIDTH);
	const uint mask      = make_mask(log_width);
	return INPUT_TEXTURE[int2(index & mask, (index >> log_width) + SOURCE_OFFSET)];
}

float4 load_weight(uint c, uint k0){
	return WEIGHT_TEXTURE[int2(c + WEIGHT_OFFSET_X, k0 + WEIGHT_OFFSET_Y)];
}

float2 load_bias(uint k0){
	return BIAS_TEXTURE[int2(k0 + BIAS_OFFSET_X, BIAS_OFFSET_Y)];
}

float4 fragment(v2f i) : SV_TARGET {
	const uint raw_index = compute_raw_index(i);

	const uint x0_width = LOG_OUTPUT_WIDTH  - 1u;
	const uint y0_width = LOG_OUTPUT_HEIGHT - 1u;
	const uint x0_mask = make_mask(x0_width);
	const uint y0_mask = make_mask(y0_width);

	const uint out_x0 = raw_index & x0_mask;
	const uint out_y0 = (raw_index >> x0_width) & y0_mask;
	const uint out_k0 = raw_index >> (x0_width + y0_width);
	if(out_x0 >= ceil_div(OUTPUT_WIDTH, 2u) || out_y0 >= ceil_div(OUTPUT_HEIGHT, 2u)){ return 0; }

	const uint x_step = 1u;
	const uint y_step = (CEIL_INPUT_WIDTH  / 2u) * x_step;
	const uint c_step = (CEIL_INPUT_HEIGHT / 2u) * y_step;
	const uint index0 =
		  out_x0 * x_step
		+ out_y0 * y_step
		+ out_k0 * c_step;

	const float2 bias = load_bias(out_k0);
	float2 acc00 = bias, acc01 = bias, acc10 = bias, acc11 = bias;

	const float4 raw_w0 = load_weight(0, out_k0);
	const float4 raw_w1 = load_weight(1, out_k0);
	const float4 raw_w2 = load_weight(2, out_k0);
	const float2 w00 = unpack_half2(raw_w0.x);
	const float2 w01 = unpack_half2(raw_w0.y);
	const float2 w02 = unpack_half2(raw_w0.z);
	const float2 w10 = unpack_half2(raw_w0.w);
	const float2 w11 = unpack_half2(raw_w1.x);
	const float2 w12 = unpack_half2(raw_w1.y);
	const float2 w20 = unpack_half2(raw_w1.z);
	const float2 w21 = unpack_half2(raw_w1.w);
	const float2 w22 = unpack_half2(raw_w2.x);

	const bool x_lo_edge = (out_x0 == 0);
	const bool y_lo_edge = (out_y0 == 0);
	const bool x_hi_edge = (out_x0 * 2u + 2u == INPUT_WIDTH);
	const bool y_hi_edge = (out_y0 * 2u + 2u == INPUT_HEIGHT);

#define ACCUMULATE(dst, s, w) \
	[unroll] \
	do { \
		dst += unpack_half2(s) * (w); \
	} while(false)

	if(!y_lo_edge){
		if(!x_lo_edge){
			const float4 input = load_input(index0 - y_step - x_step);
			ACCUMULATE(acc00, input.w, w00);
		}
		{
			const float4 input = load_input(index0 - y_step);
			ACCUMULATE(acc00, input.z, w01);
			ACCUMULATE(acc00, input.w, w02);
			ACCUMULATE(acc01, input.z, w00);
			ACCUMULATE(acc01, input.w, w01);
		}
		if(!x_hi_edge){
			const float4 input = load_input(index0 - y_step + x_step);
			ACCUMULATE(acc01, input.z, w02);
		}
	}
	{
		if(!x_lo_edge){
			const float4 input = load_input(index0 - x_step);
			ACCUMULATE(acc00, input.y, w10);
			ACCUMULATE(acc00, input.w, w20);
			ACCUMULATE(acc10, input.y, w00);
			ACCUMULATE(acc10, input.w, w10);
		}
		{
			const float4 input = load_input(index0);
			ACCUMULATE(acc00, input.x, w11);
			ACCUMULATE(acc00, input.y, w12);
			ACCUMULATE(acc00, input.z, w21);
			ACCUMULATE(acc00, input.w, w22);
			ACCUMULATE(acc01, input.x, w10);
			ACCUMULATE(acc01, input.y, w11);
			ACCUMULATE(acc01, input.z, w20);
			ACCUMULATE(acc01, input.w, w21);
			ACCUMULATE(acc10, input.x, w01);
			ACCUMULATE(acc10, input.y, w02);
			ACCUMULATE(acc10, input.z, w11);
			ACCUMULATE(acc10, input.w, w12);
			ACCUMULATE(acc11, input.x, w00);
			ACCUMULATE(acc11, input.y, w01);
			ACCUMULATE(acc11, input.z, w10);
			ACCUMULATE(acc11, input.w, w11);
		}
		if(!x_hi_edge){
			const float4 input = load_input(index0 + x_step);
			ACCUMULATE(acc01, input.x, w12);
			ACCUMULATE(acc01, input.z, w22);
			ACCUMULATE(acc11, input.x, w02);
			ACCUMULATE(acc11, input.z, w12);
		}
	}
	if(!y_hi_edge){
		if(!x_lo_edge){
			const float4 input = load_input(index0 + y_step - x_step);
			ACCUMULATE(acc10, input.y, w20);
		}
		{
			const float4 input = load_input(index0 + y_step);
			ACCUMULATE(acc10, input.x, w21);
			ACCUMULATE(acc10, input.y, w22);
			ACCUMULATE(acc11, input.x, w20);
			ACCUMULATE(acc11, input.y, w21);
		}
		if(!x_hi_edge){
			const float4 input = load_input(index0 + y_step + x_step);
			ACCUMULATE(acc11, input.x, w22);
		}
	}

#undef ACCUMULATE

	acc00 = activation(acc00);
	acc01 = activation(acc01);
	acc10 = activation(acc10);
	acc11 = activation(acc11);

	if(out_x0 * 2u + 1 >= OUTPUT_WIDTH) { acc01 = acc11 = 0; }
	if(out_y0 * 2u + 1 >= OUTPUT_HEIGHT){ acc10 = acc11 = 0; }

	return float4(
		pack_half2(acc00), pack_half2(acc01),
		pack_half2(acc10), pack_half2(acc11));
}

#endif
