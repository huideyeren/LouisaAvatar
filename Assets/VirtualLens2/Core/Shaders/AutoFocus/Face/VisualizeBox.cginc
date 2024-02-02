//---------------------------------------------------------------------------
// Visualize: Boxes
//
// Parameters:
// - INPUT_WIDTH
// - INPUT_HEIGHT
// - OUTPUT_WIDTH
// - OUTPUT_HEIGHT
// - INV_SCALE
// - PREDICTION_TEXTURE
//---------------------------------------------------------------------------

#ifndef NN4VRC_YOLOX_VISUALIZE_BOX_CGINC
#define NN4VRC_YOLOX_VISUALIZE_BOX_CGINC

#include "../Common/TargetDetector.cginc"
#include "../Common/Half.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

Texture2D<float4> PREDICTION_TEXTURE;


//---------------------------------------------------------------------------
// Structure definitions
//---------------------------------------------------------------------------

struct v2f {
	float4 vertex : SV_POSITION;
};


//---------------------------------------------------------------------------
// Vertex shader
//---------------------------------------------------------------------------

v2f vertex(uint id : SV_VERTEXID){
	v2f o;

	if(!isComputeCamera(OUTPUT_WIDTH, OUTPUT_HEIGHT)){
		o.vertex = 0;
		return o;
	}

	o.vertex = float4(id, 0, 0, 0);
	return o;
}


//---------------------------------------------------------------------------
// Geometry shader
//---------------------------------------------------------------------------

[maxvertexcount(16 * BUFFER_WIDTH)]
void geometry(point v2f input[1], inout TriangleStream<v2f> stream){
	if(!isComputeCamera(OUTPUT_WIDTH, OUTPUT_HEIGHT)){ return; }

	const uint id = input[0].vertex.x;
	if(id >= PREDICTION_ROWS){ return; }

	const float2 weight = 6.0 * float2(1.0 / OUTPUT_WIDTH, _ProjectionParams.x / OUTPUT_HEIGHT);
	const float2 scale = float2(
		2.0 * INV_SCALE / OUTPUT_WIDTH,
		2.0 * INV_SCALE / OUTPUT_HEIGHT);
	const float2 offset = float2(
		(float)(OUTPUT_WIDTH  - INPUT_WIDTH  * INV_SCALE) / OUTPUT_WIDTH  - 1.0,
		(float)(OUTPUT_HEIGHT - INPUT_HEIGHT * INV_SCALE) / OUTPUT_HEIGHT - 1.0);

	for(uint i = 0; i < BUFFER_WIDTH; ++i){
		const float4 raw = PREDICTION_TEXTURE[int2(i, id)];
		const float  obj = unpack_half2(raw.z).x;
		if(obj == 0){ continue; }
		const float2 xy = unpack_half2(raw.x);
		const float2 wh = unpack_half2(raw.y);
		const float2 lt = (xy - 0.5 * wh) * scale + offset;
		const float2 rb = (xy + 0.5 * wh) * scale + offset;
		const float l =  lt.x;
		const float r =  rb.x;
		const float t = -rb.y * _ProjectionParams.x;
		const float b = -lt.y * _ProjectionParams.x;

		{	// Top
			v2f p0, p1, p2, p3;
			p0.vertex = float4(l - weight.x, t - weight.y, 0, 1);
			p1.vertex = float4(l - weight.x, t + weight.y, 0, 1);
			p2.vertex = float4(r + weight.x, t - weight.y, 0, 1);
			p3.vertex = float4(r + weight.x, t + weight.y, 0, 1);
			stream.Append(p0);
			stream.Append(p1);
			stream.Append(p2);
			stream.Append(p3);
			stream.RestartStrip();
		}
		{	// Bottom
			v2f p0, p1, p2, p3;
			p0.vertex = float4(l - weight.x, b - weight.y, 0, 1);
			p1.vertex = float4(l - weight.x, b + weight.y, 0, 1);
			p2.vertex = float4(r + weight.x, b - weight.y, 0, 1);
			p3.vertex = float4(r + weight.x, b + weight.y, 0, 1);
			stream.Append(p0);
			stream.Append(p1);
			stream.Append(p2);
			stream.Append(p3);
			stream.RestartStrip();
		}
		{	// Left
			v2f p0, p1, p2, p3;
			p0.vertex = float4(l - weight.x, t + weight.y, 0, 1);
			p1.vertex = float4(l - weight.x, b - weight.y, 0, 1);
			p2.vertex = float4(l + weight.x, t + weight.y, 0, 1);
			p3.vertex = float4(l + weight.x, b - weight.y, 0, 1);
			stream.Append(p0);
			stream.Append(p1);
			stream.Append(p2);
			stream.Append(p3);
			stream.RestartStrip();
		}
		{	// Right
			v2f p0, p1, p2, p3;
			p0.vertex = float4(r - weight.x, t + weight.y, 0, 1);
			p1.vertex = float4(r - weight.x, b - weight.y, 0, 1);
			p2.vertex = float4(r + weight.x, t + weight.y, 0, 1);
			p3.vertex = float4(r + weight.x, b - weight.y, 0, 1);
			stream.Append(p0);
			stream.Append(p1);
			stream.Append(p2);
			stream.Append(p3);
			stream.RestartStrip();
		}
	}
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

float4 fragment(v2f i) : SV_TARGET {
	return float4(0, 1, 0, 1);
}

#endif
