#ifndef VIRTUALLENS2_COMMON_CAMERA_POSE_TEXTURE_CGINC
#define VIRTUALLENS2_COMMON_CAMERA_POSE_TEXTURE_CGINC

#include "UnityCG.cginc"

//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

Texture2D<float4> _CameraPoseTex;


//---------------------------------------------------------------------------
// Camera pose texture mapping:
//
// (0-3, 0): View matrix (world -> view)
// (0-3, 1): Inversed view matrix (view -> world)
//
//---------------------------------------------------------------------------

float4x4 getViewMatrix(){
	float4x4 ret;
	ret._m00_m01_m02_m03 = _CameraPoseTex[int2(0, 0)];
	ret._m10_m11_m12_m13 = _CameraPoseTex[int2(1, 0)];
	ret._m20_m21_m22_m23 = _CameraPoseTex[int2(2, 0)];
	ret._m30_m31_m32_m33 = _CameraPoseTex[int2(3, 0)];
	return ret;
}

float4x4 getInversedViewMatrix(){
	float4x4 ret;
	ret._m00_m01_m02_m03 = _CameraPoseTex[int2(0, 1)];
	ret._m10_m11_m12_m13 = _CameraPoseTex[int2(1, 1)];
	ret._m20_m21_m22_m23 = _CameraPoseTex[int2(2, 1)];
	ret._m30_m31_m32_m33 = _CameraPoseTex[int2(3, 1)];
	return ret;
}

#endif
