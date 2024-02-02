//---------------------------------------------------------------------------
// Decode box
//
// - BUFFER_WIDTH
// - BUFFER_HEIGHT
// - STRIDE
// - INPUT_WIDTH
// - INPUT_HEIGHT
//---------------------------------------------------------------------------

#ifndef NN4VRC_YOLOX_DECODE_BOX_CGINC
#define NN4VRC_YOLOX_DECODE_BOX_CGINC

#include "../Common/Utility.cginc"

//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

static const uint DECODE_LOG_INPUT_WIDTH_0  = ceil_log2(INPUT_WIDTH);
static const uint DECODE_LOG_INPUT_WIDTH_1  = DECODE_LOG_INPUT_WIDTH_0  - 1;
static const uint DECODE_LOG_INPUT_WIDTH_2  = DECODE_LOG_INPUT_WIDTH_1  - 1;

static const uint DECODE_LOG_INPUT_HEIGHT_0 = ceil_log2(INPUT_HEIGHT);
static const uint DECODE_LOG_INPUT_HEIGHT_1 = DECODE_LOG_INPUT_HEIGHT_0 - 1;
static const uint DECODE_LOG_INPUT_HEIGHT_2 = DECODE_LOG_INPUT_HEIGHT_1 - 1;

static const uint DECODE_CEIL_INPUT_WIDTH_0  = 1u << DECODE_LOG_INPUT_WIDTH_0;
static const uint DECODE_CEIL_INPUT_WIDTH_1  = 1u << DECODE_LOG_INPUT_WIDTH_1;
static const uint DECODE_CEIL_INPUT_WIDTH_2  = 1u << DECODE_LOG_INPUT_WIDTH_2;

static const uint DECODE_CEIL_INPUT_HEIGHT_0 = 1u << DECODE_LOG_INPUT_HEIGHT_0;
static const uint DECODE_CEIL_INPUT_HEIGHT_1 = 1u << DECODE_LOG_INPUT_HEIGHT_1;
static const uint DECODE_CEIL_INPUT_HEIGHT_2 = 1u << DECODE_LOG_INPUT_HEIGHT_2;

static const uint DECODE_OFFSET_0 = 0;
static const uint DECODE_OFFSET_1 =
	  DECODE_OFFSET_0
	+ ceil_div(DECODE_CEIL_INPUT_WIDTH_0 * DECODE_CEIL_INPUT_HEIGHT_0, BUFFER_WIDTH);
static const uint DECODE_OFFSET_2 =
	  DECODE_OFFSET_1
	+ ceil_div(DECODE_CEIL_INPUT_WIDTH_1 * DECODE_CEIL_INPUT_HEIGHT_1, BUFFER_WIDTH);


//---------------------------------------------------------------------------
// Utility functions
//---------------------------------------------------------------------------

float get_decode_stride(int2 pos){
	if((uint)pos.y >= DECODE_OFFSET_2){ return STRIDE * 4u; }
	if((uint)pos.y >= DECODE_OFFSET_1){ return STRIDE * 2u; }
	return STRIDE;
}

float2 get_decode_offset(int2 pos){
	uint index = 0, x_width = 0, y_width = 0;
	if((uint)pos.y >= DECODE_OFFSET_2){
		index = (pos.y - DECODE_OFFSET_2) * BUFFER_WIDTH + pos.x;
		x_width = DECODE_LOG_INPUT_WIDTH_2  - 1u;
		y_width = DECODE_LOG_INPUT_HEIGHT_2 - 1u;
	}else if((uint)pos.y >= DECODE_OFFSET_1){
		index = (pos.y - DECODE_OFFSET_1) * BUFFER_WIDTH + pos.x;
		x_width = DECODE_LOG_INPUT_WIDTH_1  - 1u;
		y_width = DECODE_LOG_INPUT_HEIGHT_1 - 1u;
	}else{
		index = (pos.y - DECODE_OFFSET_0) * BUFFER_WIDTH + pos.x;
		x_width = DECODE_LOG_INPUT_WIDTH_0  - 1u;
		y_width = DECODE_LOG_INPUT_HEIGHT_0 - 1u;
	}
	const uint x_mask = make_mask(x_width);
	const uint y_mask = make_mask(y_width);
	const uint x0 = index & x_mask;
	const uint y0 = (index >> x_width) & y_mask;
	const uint z  = index >> (x_width + y_width);
	const uint x  = x0 * 2 + (z & 1);
	const uint y  = y0 * 2 + (z >> 1);
	return float2(x, y);
}

#endif
