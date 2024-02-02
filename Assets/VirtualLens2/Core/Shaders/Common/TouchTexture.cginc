#ifndef VIRTUALLENS2_COMMON_TOUCH_TEXTURE_CGINC
#define VIRTUALLENS2_COMMON_TOUCH_TEXTURE_CGINC

#include "UnityCG.cginc"
#include "Constants.cginc"

//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

Texture2D<float4> _TouchScreenTex;


//---------------------------------------------------------------------------
// Touch screen texture mapping:
//
// (0, 0):
//   - X: u (not enlarged)
//   - Y: v (not enlarged)
//   - W: 0.0625 if touching
//---------------------------------------------------------------------------

bool isTouching(){
	const float4 raw = _TouchScreenTex[int2(0, 0)];
	return raw.w == 0.0625;
}

float2 getTouchingPoint(){
	const float4 raw = _TouchScreenTex[int2(0, 0)];
	const float2 scale  = INV_SCREEN_ENLARGEMENT;
	const float2 offset = (1.0 - scale) * 0.5;
	return raw.xy * scale + offset;
}

#endif
