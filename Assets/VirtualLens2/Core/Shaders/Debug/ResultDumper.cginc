#ifndef VIRTUALLENS2_DEBUG_RESULT_DUMPER_CGINC
#define VIRTUALLENS2_DEBUG_RESULT_DUMPER_CGINC

#include "UnityCG.cginc"
#include "../Common/Constants.cginc"
#include "../Common/TargetDetector.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

Texture2D _MainTex;

SamplerState point_clamp_sampler;


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

bool isTargetCamera(){
	return _ScreenParams.x == 2048.0
	    && _ScreenParams.y == 1152.0
	    && testPerspective()
	    && !testStereo()
	    && !testInMirror();
}

v2f vertex(appdata v){
	v2f o;

	if(!isTargetCamera()){
		o.vertex = 0;
		o.uv     = 0;
		return o;
	}

	const float x = (v.uv.x * 2 - 1);
	const float y = (v.uv.y * 2 - 1) * _ProjectionParams.x;
#ifdef UNITY_REVERSED_Z
	const float z = 1.0f;
#else
	const float z = UNITY_NEAR_CLIP_VALUE;
#endif
	const float internal_aspect = ASPECT;
	const float target_aspect = _ScreenParams.x / _ScreenParams.y;
	float x_scale = INV_SCREEN_ENLARGEMENT.x;
	float y_scale = INV_SCREEN_ENLARGEMENT.y;
	if(target_aspect > internal_aspect){
		y_scale *= internal_aspect / target_aspect;
	}else{
		x_scale *= target_aspect / internal_aspect;
	}
	o.vertex = float4(x, y, z, 1);
	o.uv     = v.uv;
	return o;
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

float4 fragment(v2f i) : SV_Target {
	return _MainTex.Sample(point_clamp_sampler, i.uv);
}

#endif
