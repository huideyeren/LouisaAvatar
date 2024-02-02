#ifndef VIRTUALLENS2_SYSTEM_DISPLAY_TEXT_RENDERER_CGINC
#define VIRTUALLENS2_SYSTEM_DISPLAY_TEXT_RENDERER_CGINC

#include "UnityCG.cginc"
#include "../Common/Constants.cginc"
#include "../Common/TargetDetector.cginc"
#include "../Common/FieldOfView.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

Texture2D<float4> _ComponentTex;  // Components texture

float _ShowInfo;   // Display parameters

float _FieldOfView;     // Raw field of view [deg]
float _LogFNumber;      // log(F)
float _BlurringThresh;  // Maximum F-number (disable DoF simulation when _LogFNumber == _BlurringThresh)

SamplerState point_clamp_sampler;
SamplerState linear_clamp_sampler;


//---------------------------------------------------------------------------
// Enumerates and constants
//---------------------------------------------------------------------------

static const int DISPLAY_WIDTH  = 1280;
static const int DISPLAY_HEIGHT =  720;


//---------------------------------------------------------------------------
// Structure definitions
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
// Vertex shader
//---------------------------------------------------------------------------

v2f vertex(appdata v){
	v2f o;
	o.vertex = 0;
	o.uv     = v.uv;
	return o;
}


//---------------------------------------------------------------------------
// Geometry shader
//---------------------------------------------------------------------------

// Utility: draw a rectangle
void drawRectangle(
	inout TriangleStream<v2f> output,
	float2 p0, float2 p1,
	float2 uv0, float2 uv1)
{
	const float2 scale  = 2.0 / float2(DISPLAY_WIDTH, DISPLAY_HEIGHT);
	const float2 offset = float2(1.0, 1.0);
	p0 = (p0 * scale - offset) * float2(1.0, _ProjectionParams.x);
	p1 = (p1 * scale - offset) * float2(1.0, _ProjectionParams.x);
	v2f v0, v1, v2, v3;
	v0.vertex = float4(p0.x, p0.y, 0, 1);
	v1.vertex = float4(p0.x, p1.y, 0, 1);
	v2.vertex = float4(p1.x, p0.y, 0, 1);
	v3.vertex = float4(p1.x, p1.y, 0, 1);
	v0.uv = float2(uv0.x, uv0.y);
	v1.uv = float2(uv0.x, uv1.y);
	v2.uv = float2(uv1.x, uv0.y);
	v3.uv = float2(uv1.x, uv1.y);
	output.Append(v0);
	output.Append(v1);
	output.Append(v2);
	output.Append(v3);
	output.RestartStrip();
}

// Utility: draw a character
void drawCharacter(
	inout TriangleStream<v2f> output,
	float2 p0, float2 p1, uint c)
{
	const uint row = c >> 3;
	const uint col = c &  7;
	const float2 uv0 = float2((col + 0) * 42.0 / 512.0, (row + 0) * 64.0 / 512.0);
	const float2 uv1 = float2((col + 1) * 42.0 / 512.0, (row + 1) * 64.0 / 512.0);
	drawRectangle(output, p0, p1, uv0, uv1);
}

// Draw focal length
void drawFocalLength(inout TriangleStream<v2f> output, float fov){
	const uint focal = (uint)round(computeFocalLength(fov) * 1e3);
	const uint c0 = (focal >= 100 ? focal / 100 % 10 : 15);
	const uint c1 = (focal >=  10 ? focal /  10 % 10 : 15);
	const uint c2 = (focal % 10);
	const uint c3 = 10;  // 'm'
	const uint c4 = 10;  // 'm'
	drawCharacter(output, float2(32 + 42 * 0, 16), float2(32 + 42 * 1, 80), c0);
	drawCharacter(output, float2(32 + 42 * 1, 16), float2(32 + 42 * 2, 80), c1);
	drawCharacter(output, float2(32 + 42 * 2, 16), float2(32 + 42 * 3, 80), c2);
	drawCharacter(output, float2(32 + 42 * 3, 16), float2(32 + 42 * 4, 80), c3);
	drawCharacter(output, float2(32 + 42 * 4, 16), float2(32 + 42 * 5, 80), c4);
}

// Draw F number
void drawFNumber(inout TriangleStream<v2f> output, float log_f){
	uint c0, c1, c2, c3, c4;
	if(log_f == _BlurringThresh){
		c0 = 11;  // 'F'
		c1 = 13;  // infinity.left
		c2 = 14;  // infinity.right
		c3 = 15;  // ' '
		c4 = 15;  // ' '
	}else{
		const uint F = (uint)round(exp(log_f) * 10);
		c0 = 11;  // 'F'
		c1 = (F >= 100 ? F / 100 % 10 : 15);
		c2 = (F / 10 % 10);
		c3 = 12;  // '.'
		c4 = (F % 10);
	}
	drawCharacter(output, float2(1038 + 42 * 0, 16), float2(1038 + 42 * 1, 80), c0);
	drawCharacter(output, float2(1038 + 42 * 1, 16), float2(1038 + 42 * 2, 80), c1);
	drawCharacter(output, float2(1038 + 42 * 2, 16), float2(1038 + 42 * 3, 80), c2);
	drawCharacter(output, float2(1038 + 42 * 3, 16), float2(1038 + 42 * 4, 80), c3);
	drawCharacter(output, float2(1038 + 42 * 4, 16), float2(1038 + 42 * 5, 80), c4);
}

// Entry point
[maxvertexcount(20)]
void geometry(
	triangle v2f input[3],
	in uint id : SV_PrimitiveID,
	inout TriangleStream<v2f> output)
{
	const int show_info = (int)round(_ShowInfo);
	if(!show_info || !isVirtualLensCustomComputeCamera(DISPLAY_WIDTH, DISPLAY_HEIGHT)){ return; }
	if(id == 0){
		drawFocalLength(output, _FieldOfView);
	}else if(id == 1){
		drawFNumber(output, _LogFNumber);
	}
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

float4 fragment(v2f i) : SV_Target {
	return _ComponentTex.Sample(linear_clamp_sampler, i.uv);
}

#endif
