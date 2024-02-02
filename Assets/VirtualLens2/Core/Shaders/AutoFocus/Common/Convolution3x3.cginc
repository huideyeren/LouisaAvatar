//---------------------------------------------------------------------------
// Convolution3x3
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
// - INPUT_TEXTURE
// - WEIGHT_TEXTURE
// - BIAS_TEXTURE
// - WEIGHT_OFFSET_X
// - WEIGHT_OFFSET_Y
// - BIAS_OFFSET_X
// - BIAS_OFFSET_Y
//---------------------------------------------------------------------------

#ifndef NN4VRC_COMMON_CONVOLUTION_3x3_CGINC
#define NN4VRC_COMMON_CONVOLUTION_3x3_CGINC

#include "UnityCG.cginc"
#include "Utility.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

static const uint OUTPUT_WIDTH  = INPUT_WIDTH  - 2u;
static const uint OUTPUT_HEIGHT = INPUT_HEIGHT - 2u;

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
		+ out_y0 * y_step;

	const float2 bias = load_bias(out_k0);
	float2 acc00 = bias, acc01 = bias, acc10 = bias, acc11 = bias;

	for(uint c = 0u, weight_index = 0u; c < ceil_div(INPUT_CHANNELS, 4u); ++c){
		const float4 packed_w0 = load_weight(weight_index++, out_k0);
		float2 w0_00, w0_01, w0_02;
		float2 w0_10, w0_11, w0_12;
		float2 w0_20, w0_21, w0_22;
		float2 w1_00, w1_01, w1_02;
		float2 w1_10, w1_11, w1_12;
		float2 w1_20, w1_21, w1_22;

#define LOAD_WEIGHTS(first, second) \
		[unroll] \
		do { \
			const float4 wa = load_weight(weight_index++, out_k0); \
			const float4 wb = load_weight(weight_index++, out_k0); \
			const float4 wc = load_weight(weight_index++, out_k0); \
			const float4 wd = load_weight(weight_index++, out_k0); \
			w0_00 = unpack_half2(packed_w0.first); \
			w0_01 = unpack_half2(wa.x); \
			w0_02 = unpack_half2(wa.y); \
			w0_10 = unpack_half2(wa.z); \
			w0_11 = unpack_half2(wa.w); \
			w0_12 = unpack_half2(wb.x); \
			w0_20 = unpack_half2(wb.y); \
			w0_21 = unpack_half2(wb.z); \
			w0_22 = unpack_half2(wb.w); \
			w1_00 = unpack_half2(packed_w0.second); \
			w1_01 = unpack_half2(wc.x); \
			w1_02 = unpack_half2(wc.y); \
			w1_10 = unpack_half2(wc.z); \
			w1_11 = unpack_half2(wc.w); \
			w1_12 = unpack_half2(wd.x); \
			w1_20 = unpack_half2(wd.y); \
			w1_21 = unpack_half2(wd.z); \
			w1_22 = unpack_half2(wd.w); \
		} while(false)
#define ACCUMULATE(dst, s, wi) \
		[unroll] \
		do { \
			dst += unpack_half2(s).x * (w0 ## wi); \
			dst += unpack_half2(s).y * (w1 ## wi); \
		} while(false)

		{
			const uint index = index0 + (c * 2 + 0) * c_step;
			LOAD_WEIGHTS(x, y);
			const float4 in00 = load_input(index);
			ACCUMULATE(acc00, in00.x, _00);
			ACCUMULATE(acc00, in00.y, _01);
			ACCUMULATE(acc00, in00.z, _10);
			ACCUMULATE(acc00, in00.w, _11);
			ACCUMULATE(acc01, in00.y, _00);
			ACCUMULATE(acc01, in00.w, _10);
			ACCUMULATE(acc10, in00.z, _00);
			ACCUMULATE(acc10, in00.w, _01);
			ACCUMULATE(acc11, in00.w, _00);
			const float4 in01 = load_input(index + x_step);
			ACCUMULATE(acc00, in01.x, _02);
			ACCUMULATE(acc00, in01.z, _12);
			ACCUMULATE(acc01, in01.x, _01);
			ACCUMULATE(acc01, in01.y, _02);
			ACCUMULATE(acc01, in01.z, _11);
			ACCUMULATE(acc01, in01.w, _12);
			ACCUMULATE(acc10, in01.z, _02);
			ACCUMULATE(acc11, in01.z, _01);
			ACCUMULATE(acc11, in01.w, _02);
			const float4 in10 = load_input(index + y_step);
			ACCUMULATE(acc00, in10.x, _20);
			ACCUMULATE(acc00, in10.y, _21);
			ACCUMULATE(acc01, in10.y, _20);
			ACCUMULATE(acc10, in10.x, _10);
			ACCUMULATE(acc10, in10.y, _11);
			ACCUMULATE(acc10, in10.z, _20);
			ACCUMULATE(acc10, in10.w, _21);
			ACCUMULATE(acc11, in10.y, _10);
			ACCUMULATE(acc11, in10.w, _20);
			const float4 in11 = load_input(index + y_step + x_step);
			ACCUMULATE(acc00, in11.x, _22);
			ACCUMULATE(acc01, in11.x, _21);
			ACCUMULATE(acc01, in11.y, _22);
			ACCUMULATE(acc10, in11.x, _12);
			ACCUMULATE(acc10, in11.z, _22);
			ACCUMULATE(acc11, in11.x, _11);
			ACCUMULATE(acc11, in11.y, _12);
			ACCUMULATE(acc11, in11.z, _21);
			ACCUMULATE(acc11, in11.w, _22);
		}

		{
			const uint index = index0 + (c * 2 + 1) * c_step;
			LOAD_WEIGHTS(z, w);
			const float4 in00 = load_input(index);
			ACCUMULATE(acc00, in00.x, _00);
			ACCUMULATE(acc00, in00.y, _01);
			ACCUMULATE(acc00, in00.z, _10);
			ACCUMULATE(acc00, in00.w, _11);
			ACCUMULATE(acc01, in00.y, _00);
			ACCUMULATE(acc01, in00.w, _10);
			ACCUMULATE(acc10, in00.z, _00);
			ACCUMULATE(acc10, in00.w, _01);
			ACCUMULATE(acc11, in00.w, _00);
			const float4 in01 = load_input(index + x_step);
			ACCUMULATE(acc00, in01.x, _02);
			ACCUMULATE(acc00, in01.z, _12);
			ACCUMULATE(acc01, in01.x, _01);
			ACCUMULATE(acc01, in01.y, _02);
			ACCUMULATE(acc01, in01.z, _11);
			ACCUMULATE(acc01, in01.w, _12);
			ACCUMULATE(acc10, in01.z, _02);
			ACCUMULATE(acc11, in01.z, _01);
			ACCUMULATE(acc11, in01.w, _02);
			const float4 in10 = load_input(index + y_step);
			ACCUMULATE(acc00, in10.x, _20);
			ACCUMULATE(acc00, in10.y, _21);
			ACCUMULATE(acc01, in10.y, _20);
			ACCUMULATE(acc10, in10.x, _10);
			ACCUMULATE(acc10, in10.y, _11);
			ACCUMULATE(acc10, in10.z, _20);
			ACCUMULATE(acc10, in10.w, _21);
			ACCUMULATE(acc11, in10.y, _10);
			ACCUMULATE(acc11, in10.w, _20);
			const float4 in11 = load_input(index + y_step + x_step);
			ACCUMULATE(acc00, in11.x, _22);
			ACCUMULATE(acc01, in11.x, _21);
			ACCUMULATE(acc01, in11.y, _22);
			ACCUMULATE(acc10, in11.x, _12);
			ACCUMULATE(acc10, in11.z, _22);
			ACCUMULATE(acc11, in11.x, _11);
			ACCUMULATE(acc11, in11.y, _12);
			ACCUMULATE(acc11, in11.z, _21);
			ACCUMULATE(acc11, in11.w, _22);
		}

#undef LOAD_WEIGHTS
#undef ACCUMULATE
	}

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
