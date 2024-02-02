//---------------------------------------------------------------------------
// MaxPool5x5 (pad=2)
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

#ifndef NN4VRC_COMMON_MAX_POOL_5x5p2_CGINC
#define NN4VRC_COMMON_MAX_POOL_5x5p2_CGINC

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

	float2 acc_l = -1.#INF, acc_r = -1.#INF, acc_u = -1.#INF, acc_d = -1.#INF, acc_c = -1.#INF;
	float2 acc00 = -1.#INF, acc01 = -1.#INF, acc10 = -1.#INF, acc11 = -1.#INF;

	const int r = 1;
	[unroll]
	for(int dy = -r; dy <= r; ++dy){
		const int y = (int)out_y0 + dy;
		if(y < 0 || (int)ceil_div(INPUT_HEIGHT, 2u) <= y){ continue; }
		[unroll]
		for(int dx = -r; dx <= r; ++dx){
			const int x = (int)out_x0 + dx;
			if(x < 0 || (int)ceil_div(INPUT_WIDTH, 2u) <= x){ continue; }
			const float4 values = load_input((int)index0 + dy * (int)y_step + dx * (int)x_step);
			if(dy == -r){
				if(dx == -r){
					acc00 = unpack_half2(values.x);
					acc_u = max(acc_u, unpack_half2(values.y));
					acc_l = max(acc_l, unpack_half2(values.z));
					acc_c = max(acc_c, unpack_half2(values.w));
				}else if(dx != r){
					acc_u = max(acc_u, unpack_half2(values.x));
					acc_u = max(acc_u, unpack_half2(values.y));
					acc_c = max(acc_c, unpack_half2(values.z));
					acc_c = max(acc_c, unpack_half2(values.w));
				}else{
					acc_u = max(acc_u, unpack_half2(values.x));
					acc01 = unpack_half2(values.y);
					acc_c = max(acc_c, unpack_half2(values.z));
					acc_r = max(acc_r, unpack_half2(values.w));
				}
			}else if(dy != r){
				if(dx == -r){
					acc_l = max(acc_l, unpack_half2(values.x));
					acc_c = max(acc_c, unpack_half2(values.y));
					acc_l = max(acc_l, unpack_half2(values.z));
					acc_c = max(acc_c, unpack_half2(values.w));
				}else if(dx != r){
					acc_c = max(acc_c, unpack_half2(values.x));
					acc_c = max(acc_c, unpack_half2(values.y));
					acc_c = max(acc_c, unpack_half2(values.z));
					acc_c = max(acc_c, unpack_half2(values.w));
				}else{
					acc_c = max(acc_c, unpack_half2(values.x));
					acc_r = max(acc_r, unpack_half2(values.y));
					acc_c = max(acc_c, unpack_half2(values.z));
					acc_r = max(acc_r, unpack_half2(values.w));
				}
			}else{
				if(dx == -r){
					acc_l = max(acc_l, unpack_half2(values.x));
					acc_c = max(acc_c, unpack_half2(values.y));
					acc10 = unpack_half2(values.z);
					acc_d = max(acc_d, unpack_half2(values.w));
				}else if(dx != r){
					acc_c = max(acc_c, unpack_half2(values.x));
					acc_c = max(acc_c, unpack_half2(values.y));
					acc_d = max(acc_d, unpack_half2(values.z));
					acc_d = max(acc_d, unpack_half2(values.w));
				}else{
					acc_c = max(acc_c, unpack_half2(values.x));
					acc_r = max(acc_r, unpack_half2(values.y));
					acc_d = max(acc_d, unpack_half2(values.z));
					acc11 = unpack_half2(values.w);
				}
			}
		}
	}

	acc00 = max(acc00, max(max(acc_u, acc_c), acc_l));
	acc01 = max(acc01, max(max(acc_u, acc_c), acc_r));
	acc10 = max(acc10, max(max(acc_d, acc_c), acc_l));
	acc11 = max(acc11, max(max(acc_d, acc_c), acc_r));

	if(out_x0 * 2u + 1 >= OUTPUT_WIDTH) { acc01 = acc11 = 0; }
	if(out_y0 * 2u + 1 >= OUTPUT_HEIGHT){ acc10 = acc11 = 0; }

	return float4(
		pack_half2(acc00), pack_half2(acc01),
		pack_half2(acc10), pack_half2(acc11));
}

#endif
