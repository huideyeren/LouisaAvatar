//---------------------------------------------------------------------------
// Rearrange + Padding for YOLOX
//
// Parameters:
// - BUFFER_WIDTH
// - BUFFER_HEIGHT
// - SOURCE_OFFSET
// - DESTINATION_OFFSET
// - INPUT_WIDTH
// - INPUT_HEIGHT
// - INPUT_TEXTURE
//---------------------------------------------------------------------------

#ifndef NN4VRC_YOLOX_REARRANGE_CGINC
#define NN4VRC_YOLOX_REARRANGE_CGINC

#include "UnityCG.cginc"
#include "../Common/Utility.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

static const uint OUTPUT_WIDTH    = ceil_div(INPUT_WIDTH,  2u) + 2u;
static const uint OUTPUT_HEIGHT   = ceil_div(INPUT_HEIGHT, 2u) + 2u;
static const uint OUTPUT_CHANNELS = 16u;

static const uint LOG_INPUT_WIDTH   = ceil_log2(INPUT_WIDTH);
static const uint LOG_INPUT_HEIGHT  = ceil_log2(INPUT_HEIGHT);
static const uint LOG_OUTPUT_WIDTH  = ceil_log2(OUTPUT_WIDTH);
static const uint LOG_OUTPUT_HEIGHT = ceil_log2(OUTPUT_HEIGHT);

static const uint CEIL_INPUT_WIDTH   = 1u << LOG_INPUT_WIDTH;
static const uint CEIL_INPUT_HEIGHT  = 1u << LOG_INPUT_HEIGHT;
static const uint CEIL_OUTPUT_WIDTH  = 1u << LOG_OUTPUT_WIDTH;
static const uint CEIL_OUTPUT_HEIGHT = 1u << LOG_OUTPUT_HEIGHT;

Texture2D<float4> INPUT_TEXTURE;


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

float4 load_input(int x, int y){
	if(x < 0 || INPUT_WIDTH  <= x){ return 0; }
	if(y < 0 || INPUT_HEIGHT <= y){ return 0; }
	const uint log_width = ceil_log2(BUFFER_WIDTH);
	const uint mask      = make_mask(log_width);
	const uint index     = x + y * CEIL_INPUT_WIDTH;
	return INPUT_TEXTURE[int2(index & mask, (index >> log_width) + SOURCE_OFFSET)];
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

	const uint offset_x = (out_k0 >> 2) & 1;
	const uint offset_y = (out_k0 >> 1) & 1;
	const uint offset_k = out_k0 & 1;

	const float4 in00 = load_input((int)out_x0 * 4 - 2 + offset_x, (int)out_y0 * 4 - 2 + offset_y);
	const float4 in01 = load_input((int)out_x0 * 4 - 0 + offset_x, (int)out_y0 * 4 - 2 + offset_y);
	const float4 in10 = load_input((int)out_x0 * 4 - 2 + offset_x, (int)out_y0 * 4 - 0 + offset_y);
	const float4 in11 = load_input((int)out_x0 * 4 - 0 + offset_x, (int)out_y0 * 4 - 0 + offset_y);
	const float2 acc00 = offset_k ? in00.zw : in00.xy;
	const float2 acc01 = offset_k ? in01.zw : in01.xy;
	const float2 acc10 = offset_k ? in10.zw : in10.xy;
	const float2 acc11 = offset_k ? in11.zw : in11.xy;

	return float4(
		pack_half2(acc00), pack_half2(acc01),
		pack_half2(acc10), pack_half2(acc11));
}


#endif
