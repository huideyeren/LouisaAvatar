#ifndef VIRTUALLENS2_AUTO_FOCUS_AUTO_FOCUS_TEXTURE_CGINC
#define VIRTUALLENS2_AUTO_FOCUS_AUTO_FOCUS_TEXTURE_CGINC

#include "UnityCG.cginc"

//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

Texture2D<float4> _AutoFocusTex;


//---------------------------------------------------------------------------
// Auto focus texture mapping:
//
// (0, 0): Face boundary box (enlarged uv space)
//   - X: X-coordinates of center
//   - Y: Y-coordinates of center
//   - Z: Width
//   - W: Height
//   - Z and W will be zero if detection was failed.
//
// (1, 0): Face boundary box (image space)
//   - X: X-coordinates of center
//   - Y: Y-coordinates of center
//   - Z: Width
//   - W: Height
//   - Z and W will be zero if detection was failed.
//
// (2, 0): Face focusing point
//   - XYZ: Coordinates in view space
//   - W:   Detected?
//
// (4, 0): Eye boundary box (enlarged uv space)
//   - X: X-coordinates of center
//   - Y: Y-coordinates of center
//   - Z: Width
//   - W: Height
//   - Z and W will be zero if detection was failed.
//
// (5, 0): Eye boundary box (image space)
//   - X: X-coordinates of center
//   - Y: Y-coordinates of center
//   - Z: Width
//   - W: Height
//   - Z and W will be zero if detection was failed.
//
// (6, 0): Eye focusing point
//   - XYZ: Coordinates in view space
//   - W:   Detected?
//
// (8, 0): Pixel based focusing point
//   - XYZ: Coordinates in view space
//   - W:   Detected?
//
//---------------------------------------------------------------------------

float4 getFaceBoundaryBox(){
	return _AutoFocusTex[int2(0, 0)];
}

float4 getFaceFocusingPoint(){
	return _AutoFocusTex[int2(2, 0)];
}

float4 getEyeBoundaryBox(){
	return _AutoFocusTex[int2(4, 0)];
}

float4 getEyeFocusingPoint(){
	return _AutoFocusTex[int2(6, 0)];
}

float4 getPixelFocusingPoint(){
	return _AutoFocusTex[int2(8, 0)];
}

#endif
