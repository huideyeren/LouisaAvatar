//---------------------------------------------------------------------------
// Convolution1x1
//
// Parameters:
// - BUFFER_WIDTH
// - BUFFER_HEIGHT
// - SOURCE_OFFSET
// - DESTINATION_OFFSET
// - INPUT_WIDTH
// - INPUT_HEIGHT
// - INPUT_CHANNELS
// - OUTPUT_CHANNELS
// - FUSED_ADD
// - INPUT_TEXTURE
// - WEIGHT_TEXTURE
// - BIAS_TEXTURE
// - WEIGHT_OFFSET_X
// - WEIGHT_OFFSET_Y
// - BIAS_OFFSET_X
// - BIAS_OFFSET_Y
//---------------------------------------------------------------------------

#ifndef NN4VRC_COMMON_CONVOLUTION_1x1_CGINC
#define NN4VRC_COMMON_CONVOLUTION_1x1_CGINC

#include "UnityCG.cginc"
#include "Utility.cginc"


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
Texture2D<float4> WEIGHT_TEXTURE;
Texture2D<float4> BIAS_TEXTURE;


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

float4 load_output(uint index){
	const uint log_width = ceil_log2(BUFFER_WIDTH);
	const uint mask      = make_mask(log_width);
	return INPUT_TEXTURE[int2(index & mask, (index >> log_width) + DESTINATION_OFFSET)];
}

float4 load_weight(uint k0, uint c0){
	return WEIGHT_TEXTURE[int2(k0 + WEIGHT_OFFSET_X, c0 + WEIGHT_OFFSET_Y)];
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
		+ out_y0 * y_step;

	const float2 bias = load_bias(out_k0);
	float2 acc00 = bias, acc01 = bias, acc10 = bias, acc11 = bias;

	for(uint c0 = 0; c0 < ceil_div(INPUT_CHANNELS, 4u); ++c0){
		const float4 weights = load_weight(c0, out_k0);

#define ACCUMULATE(c1, w1, w2) \
		[unroll] \
		do { \
			const float4 x = load_input(index0 + c_step * (c0 * 2u + c1)); \
			acc00 += unpack_half2(x.x).x * w1; \
			acc00 += unpack_half2(x.x).y * w2; \
			acc01 += unpack_half2(x.y).x * w1; \
			acc01 += unpack_half2(x.y).y * w2; \
			acc10 += unpack_half2(x.z).x * w1; \
			acc10 += unpack_half2(x.z).y * w2; \
			acc11 += unpack_half2(x.w).x * w1; \
			acc11 += unpack_half2(x.w).y * w2; \
		} while(false)

		ACCUMULATE(0u, unpack_half2(weights.x), unpack_half2(weights.y));
		ACCUMULATE(1u, unpack_half2(weights.z), unpack_half2(weights.w));

#undef ACCUMULATE
	}

	acc00 = activation(acc00);
	acc01 = activation(acc01);
	acc10 = activation(acc10);
	acc11 = activation(acc11);

#ifdef FUSED_ADD
	{
		const float4 output = load_output(raw_index);
		acc00 += unpack_half2(output.x);
		acc01 += unpack_half2(output.y);
		acc10 += unpack_half2(output.z);
		acc11 += unpack_half2(output.w);
	}
#endif

	if(out_x0 * 2u + 1 >= OUTPUT_WIDTH) { acc01 = acc11 = 0; }
	if(out_y0 * 2u + 1 >= OUTPUT_HEIGHT){ acc10 = acc11 = 0; }

	return float4(
		pack_half2(acc00), pack_half2(acc01),
		pack_half2(acc10), pack_half2(acc11));
}


#endif
