#ifndef VIRTUALLENS2_SYSTEM_DEPTH_CLEANER_CGINC
#define VIRTUALLENS2_SYSTEM_DEPTH_CLEANER_CGINC

#include "UnityCG.cginc"
#include "../Common/Constants.cginc"
#include "../Common/TargetDetector.cginc"


//---------------------------------------------------------------------------
// Structure definitions
//---------------------------------------------------------------------------

struct appdata {
	float4 vertex : POSITION;
	float2 uv     : TEXCOORD0;
};

struct v2f {
	float4 vertex : SV_POSITION;
};


//---------------------------------------------------------------------------
// Vertex shader
//---------------------------------------------------------------------------

v2f vertex(appdata v){
	v2f o;

	if(!isVRChatCamera() && !isVRChatCamera4K()){
		o.vertex = 0;
		return o;
	}

	const float x = (v.uv.x * 2 - 1);
	const float y = (v.uv.y * 2 - 1) * _ProjectionParams.x;
#ifdef UNITY_REVERSED_Z
	const float z = 1.0f;
#else
	const float z = UNITY_NEAR_CLIP_VALUE;
#endif
	o.vertex = float4(x, y, z, 1);
	return o;
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

// Write depth only
void fragment(v2f i){ }

#endif
