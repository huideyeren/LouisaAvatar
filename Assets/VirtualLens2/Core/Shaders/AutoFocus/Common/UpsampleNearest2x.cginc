//---------------------------------------------------------------------------
// Upsample (nearest, 2x)
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
//---------------------------------------------------------------------------

#ifndef NN4VRC_COMMON_UPSAMPLE_NEAREST_2x_CGINC
#define NN4VRC_COMMON_UPSAMPLE_NEAREST_2x_CGINC

#include "UnityCG.cginc"
#include "Utility.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

static const uint OUTPUT_WIDTH    = INPUT_WIDTH  * 2u;
static const uint OUTPUT_HEIGHT   = INPUT_HEIGHT * 2u;
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
		  (out_x0 / 2u) * x_step
		+ (out_y0 / 2u) * y_step
		+ out_k0        * c_step;

	const float4 input = load_input(index0);
	float2 output = 0;
	if((out_y0 & 1u) == 0u){
		if((out_x0 & 1u) == 0u){
			output = unpack_half2(input.x);
		}else{
			output = unpack_half2(input.y);
		}
	}else{
		if((out_x0 & 1u) == 0u){
			output = unpack_half2(input.z);
		}else{
			output = unpack_half2(input.w);
		}
	}

	return float4(
		pack_half2(output), pack_half2(output),
		pack_half2(output), pack_half2(output));
}

#endif
