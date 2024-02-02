#ifndef VIRTURLLENS2_SYSTEM_CAMERA_POSE_RECEIVER_CGINC
#define VIRTURLLENS2_SYSTEM_CAMERA_POSE_RECEIVER_CGINC

#include "UnityCG.cginc"
#include "../Common/Constants.cginc"
#include "../Common/TargetDetector.cginc"


//---------------------------------------------------------------------------
// Parameters
//---------------------------------------------------------------------------

float _OffsetY;


//---------------------------------------------------------------------------
// Struct definitions
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

	if(!isVirtualLensCustomComputeCamera(4, 2)){
		o.vertex = 0;
		o.uv     = 0;
		return o;
	}

	const float x = (v.uv.x * 2 - 1);
	const float y = (v.uv.y * 2 - 1) * _ProjectionParams.x;
#ifdef UNITY_REVERSED_Z
	const float z = 1;
#else
	const float z = UNITY_NEAR_CLIP_VALUE;
#endif
	o.vertex = float4(x, y, z, 1);
	o.uv     = v.uv;
	return o;
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

// Matrix inversion for view/camera matrix
float4x4 inverse(float4x4 s){
	float4x4 d;
	// Rotation
	d._m00 = s._m00;  d._m01 = s._m10;  d._m02 = s._m20;
	d._m10 = s._m01;  d._m11 = s._m11;  d._m12 = s._m21;
	d._m20 = s._m02;  d._m21 = s._m12;  d._m22 = s._m22;
	// Translation
	d._m03 = -(d._m00 * s._m03 + d._m01 * s._m13 + d._m02 * s._m23);
	d._m13 = -(d._m10 * s._m03 + d._m11 * s._m13 + d._m12 * s._m23);
	d._m23 = -(d._m20 * s._m03 + d._m21 * s._m13 + d._m22 * s._m23);
	// Last row
	d._m30 = d._m31 = d._m32 = 0;  d._m33 = 1;
	return d;
}

float4 fragment(v2f i) : SV_Target {
	const uint2 coord = floor(i.uv * _ScreenParams.xy);
	// Right-handed to left-handed
	float4x4 raw = UNITY_MATRIX_V;
	raw._m20 *= -1;
	raw._m21 *= -1;
	raw._m22 *= -1;
	raw._m23 *= -1;
	float4x4 pose = inverse(raw);
	pose._m13 -= _OffsetY;  // Shift translation Y
	if(coord.y == 0){
		const float4x4 view = inverse(pose);
		if(coord.x == 0){
			return view._m00_m01_m02_m03;
		}else if(coord.x == 1){
			return view._m10_m11_m12_m13;
		}else if(coord.x == 2){
			return view._m20_m21_m22_m23;
		}else{
			return view._m30_m31_m32_m33;
		}
	}else{
		if(coord.x == 0){
			return pose._m00_m01_m02_m03;
		}else if(coord.x == 1){
			return pose._m10_m11_m12_m13;
		}else if(coord.x == 2){
			return pose._m20_m21_m22_m23;
		}else{
			return pose._m30_m31_m32_m33;
		}
	}
	return float4(0, 0, 0, 1);
}

#endif
