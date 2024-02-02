//---------------------------------------------------------------------------
// Visualize: Input Image
//
// Parameters:
// - INPUT_WIDTH
// - INPUT_HEIGHT
// - OUTPUT_WIDTH
// - OUTPUT_HEIGHT
// - INPUT_TEXTURE
//---------------------------------------------------------------------------

#ifndef NN4VRC_YOLOX_VISUALIZE_SOURCE_CGINC
#define NN4VRC_YOLOX_VISUALIZE_SOURCE_CGINC

#include "../Common/TargetDetector.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

Texture2D<float4> INPUT_TEXTURE;

SamplerState point_clamp_sampler;


//---------------------------------------------------------------------------
// Structure definitions
//---------------------------------------------------------------------------

struct v2f {
	float4 vertex : SV_POSITION;
	float2 uv     : TEXCOORD0;
};


//---------------------------------------------------------------------------
// Vertex shader
//---------------------------------------------------------------------------

v2f vertex(uint id : SV_VERTEXID){
	v2f o;

	if(!isComputeCamera(OUTPUT_WIDTH, OUTPUT_HEIGHT)){
		o.vertex = 0;
		o.uv     = 0;
		return o;
	}

	o.vertex = 0;
	o.uv     = float2(id, 0);
	return o;
}


//---------------------------------------------------------------------------
// Geometry shader
//---------------------------------------------------------------------------

[maxvertexcount(4)]
void geometry(point v2f input[1], inout TriangleStream<v2f> stream){
	if(!isComputeCamera(OUTPUT_WIDTH, OUTPUT_HEIGHT)){ return; }
	if(input[0].uv.x != 0){ return; }

	const float2 uv_scale = float2(
		(float)OUTPUT_WIDTH  / (float)INPUT_WIDTH,
		(float)OUTPUT_HEIGHT / (float)INPUT_HEIGHT);
	const float2 uv_offset = (1.0 - uv_scale) * 0.5;

	v2f p0, p1, p2, p3;
	p0.vertex = float4(-1, -1 * _ProjectionParams.x, 0, 1);
	p1.vertex = float4(-1,  1 * _ProjectionParams.x, 0, 1);
	p2.vertex = float4( 1, -1 * _ProjectionParams.x, 0, 1);
	p3.vertex = float4( 1,  1 * _ProjectionParams.x, 0, 1);
	p0.uv = float2(0, 0) * uv_scale + uv_offset;
	p1.uv = float2(0, 1) * uv_scale + uv_offset;
	p2.uv = float2(1, 0) * uv_scale + uv_offset;
	p3.uv = float2(1, 1) * uv_scale + uv_offset;

	stream.Append(p0);
	stream.Append(p1);
	stream.Append(p2);
	stream.Append(p3);
	stream.RestartStrip();
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

float4 fragment(v2f i) : SV_TARGET {
	return INPUT_TEXTURE.Sample(point_clamp_sampler, i.uv);
}

#endif
