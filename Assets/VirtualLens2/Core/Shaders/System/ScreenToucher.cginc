#ifndef VIRTUALLENS2_SYSTEM_SCREEN_TOUCHER_CGINC
#define VIRTUALLENS2_SYSTEM_SCREEN_TOUCHER_CGINC

#include "UnityCG.cginc"
#include "../Common/Constants.cginc"
#include "../Common/TargetDetector.cginc"


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

	if(!isVirtualLensPreciseTouchDetectorCamera()){
		o.vertex = 0;
		o.uv     = 0;
		return o;
	}

	const float2 raw = UnityObjectToClipPos(float4(0, 0, 0, 1)).xy;
	if(raw.y < -RCP_ASPECT || RCP_ASPECT < raw.y){
		o.vertex = 0;
		o.uv     = 0;
		return o;
	}

	const float x = (v.uv.x * 2 - 1);
	const float y = (v.uv.y * 2 - 1) * _ProjectionParams.x;
	o.vertex = float4(x, y, 0, 1);
	o.uv     = raw.xy * float2(0.5, 0.5 * ASPECT) + 0.5;
	return o;
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

float4 fragment(v2f i) : SV_Target {
	return float4(1.0 - i.uv, 0, 0.0625);
}

#endif
