#ifndef VIRTUALLENS2_AUTO_FOCUS_SELFIE_SELFIE_FOCUS_MARKER_CGINC
#define VIRTUALLENS2_AUTO_FOCUS_SELFIE_SELFIE_FOCUS_MARKER_CGINC

#include "UnityCG.cginc"
#include "../../Common/TargetDetector.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

int _EyeIndex;


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

bool isSelfEyeDetectionCamera(){
	return _ScreenParams.x == 2           // Screen width
	    && _ScreenParams.y == 1           // Screen height
		&& unity_OrthoParams.y == 1.3125  // VirtualLens2 magic
		&& testPerspective()              // Perspective
		&& !testStereo()                  // Monocular
		&& !testInMirror();               // Not in mirror
}

v2f vertex(appdata v){
	v2f o;

	if(!isSelfEyeDetectionCamera()){
		o.vertex = 0;
		o.uv     = 0;
		return o;
	}

	const float3 pos = UnityObjectToViewPos(float3(0, 0, 0));
	const float x = v.uv.x - 1 + _EyeIndex;
	const float y = (v.uv.y * 2 - 1) * _ProjectionParams.x;
	o.vertex = float4(x, y, 0, 1);
	o.uv     = float2(-pos.z, 0);
	return o;
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

float4 fragment(v2f i) : SV_Target {
	return float4(i.uv, 0, 0.0625);
}

#endif
