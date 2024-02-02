#ifndef NN4VRC_COMMON_UTILITY_CGINC
#define NN4VRC_COMMON_UTILITY_CGINC

#include "TargetDetector.cginc"
#include "Half.cginc"


//---------------------------------------------------------------------------
// Structure Definition
//---------------------------------------------------------------------------

struct appdata {
	float4 vertex : POSITION;
	float2 uv     : TEXCOORD0;
};

struct v2f {
	float4 vertex : SV_POSITION;
	float2 uv     : TEXCOORD0;
};


//---------------------------------------------------------------------------
// Integer computation
//---------------------------------------------------------------------------

uint ceil_log2(uint x){
	return (uint)ceil(log2((float)x));
}

uint ceil_div(uint x, uint y){
	return (x + y - 1) / y;
}

uint make_mask(uint shift){
	return (1u << shift) - 1u;
}


//---------------------------------------------------------------------------
// Vertex shader
//---------------------------------------------------------------------------

v2f vertex_impl(appdata v, uint num_outputs){
	v2f o;

	if(!isComputeCamera(BUFFER_WIDTH, BUFFER_HEIGHT)){
		o.vertex = 0;
		o.uv     = 0;
		return o;
	}

	const uint output_rows = num_outputs / (8u * BUFFER_WIDTH);
	const float height = (float)output_rows / BUFFER_HEIGHT;
	const float offset = (float)DESTINATION_OFFSET / BUFFER_HEIGHT;

	const float x = v.uv.x;
	const float y = v.uv.y * height + offset;

	const float proj_x = (x * 2 - 1);
	const float proj_y = (y * 2 - 1) * _ProjectionParams.x;
	o.vertex = float4(proj_x, proj_y, 0, 1);
	o.uv     = float2(v.uv.x, v.uv.y * height);
	return o;
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

uint2 compute_raw_coord(v2f i){
	return uint2(floor(i.uv.x * BUFFER_WIDTH), floor(i.uv.y * BUFFER_HEIGHT));
}

uint compute_raw_index(v2f i){
	const uint2 coord = compute_raw_coord(i);
	return coord.y * BUFFER_WIDTH + coord.x;
}


#endif
