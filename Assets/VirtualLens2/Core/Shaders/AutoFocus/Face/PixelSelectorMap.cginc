//---------------------------------------------------------------------------
// Pixel Selector (map step)
//
// Parameters:
// - BUFFER_WIDTH
// - BUFFER_HEIGHT
// - DESTINATION_OFFSET
// - INPUT_WIDTH
// - INPUT_HEIGHT
// - OUTPUT_WIDTH
// - OUTPUT_HEIGHT
// - INV_SCALE
// - INPUT_TEXTURE
// - DEPTH_TEXTURE
//---------------------------------------------------------------------------

#ifndef NN4VRC_YOLOX_PIXEL_SELECTOR_MAP_CGINC
#define NN4VRC_YOLOX_PIXEL_SELECTOR_MAP_CGINC

#include "UnityCG.cginc"
#include "../Common/Utility.cginc"
#include "../../Common/Constants.cginc"
#include "../../Common/StateTexture.cginc"
#include "../../Common/TouchTexture.cginc"
#include "../../Common/CameraPoseTexture.cginc"
#include "TrackingParameters.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

float _Near;
float _Far;
float _FieldOfView;

static const uint LOG_OUTPUT_WIDTH  = ceil_log2(OUTPUT_WIDTH);
static const uint LOG_OUTPUT_HEIGHT = ceil_log2(OUTPUT_HEIGHT);

static const uint CEIL_OUTPUT_WIDTH  = 1u << LOG_OUTPUT_WIDTH;
static const uint CEIL_OUTPUT_HEIGHT = 1u << LOG_OUTPUT_HEIGHT;

static const uint INPUT_OFFSET_X = (INPUT_WIDTH  - OUTPUT_WIDTH  * INV_SCALE) / 2;
static const uint INPUT_OFFSET_Y = (INPUT_HEIGHT - OUTPUT_HEIGHT * INV_SCALE) / 2;

Texture2D<float4> INPUT_TEXTURE;
Texture2D<float>  DEPTH_TEXTURE;


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

float linearize_depth(float d){
	const float z = 1.0 / _Far - 1.0 / _Near;
	const float w = 1.0 / _Near;
	return 1.0 / (z * (1.0 - d) + w);
}

float4 fragment(v2f i) : SV_TARGET {
	const uint raw_index = compute_raw_index(i);

	const uint x_width = LOG_OUTPUT_WIDTH;
	const uint y_width = LOG_OUTPUT_HEIGHT;
	const uint x_mask = make_mask(x_width);
	const uint y_mask = make_mask(y_width);

	const uint out_x = raw_index &  x_mask;
	const uint out_y = raw_index >> x_width;
	if(out_x >= OUTPUT_WIDTH || out_y >= OUTPUT_HEIGHT){ return 0; }

	const uint in_x0 = (out_x * INV_SCALE) + INPUT_OFFSET_X;
	const uint in_y0 = (out_y * INV_SCALE) + INPUT_OFFSET_Y;

	const float4x4 v2w = getInversedViewMatrix();
	const float3 last_focus = getFocusPointInWorld();

	const float y_scale = tan((PI - _FieldOfView * DEG2RAD) * 0.5);
	const float x_scale = y_scale / ASPECT;

	const uint step = max(1u, INV_SCALE / 2u);
	float4 best = float4(0, 0, 0, 1.#INF);
	[unroll]
	for(int dy = 0; dy < INV_SCALE; dy += step){
		[unroll]
		for(int dx = 0; dx < INV_SCALE; dx += step){
			const int image_x = in_x0 + dx;
			const int image_y = in_y0 + dy;
			const float proj_d = DEPTH_TEXTURE[int2(image_x, image_y)];
			if(proj_d == 0){ continue; }
			const float proj_x = (image_x * rcp(INPUT_WIDTH))  * 2.0 - 1.0;
			const float proj_y = (image_y * rcp(INPUT_HEIGHT)) * 2.0 - 1.0;
			const float z = linearize_depth(proj_d);
			const float x = proj_x * z / x_scale;
			const float y = proj_y * z / y_scale;
			const float s = length(mul(v2w, float4(x, y, z, 1)).xyz - last_focus);
			if(s < get_pixel_tracking_distance_limit() && s < best.w){
				best = float4(x, y, z, s);
			}
		}
	}
	return best;
}


#endif
