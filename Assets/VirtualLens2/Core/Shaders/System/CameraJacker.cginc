#ifndef VIRTUALLENS2_SYSTEM_CAMERA_JACKER_CGINC
#define VIRTUALLENS2_SYSTEM_CAMERA_JACKER_CGINC

#include "UnityCG.cginc"
#include "../Common/Constants.cginc"
#include "../Common/TargetDetector.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

float _Ignore4KScreen;

Texture2D _MainTex;

SamplerState linear_clamp_sampler;


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

	if(!isVRChatCamera() && (_Ignore4KScreen != 0 || !isVRChatCamera4K())){
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
	o.uv.x = v.uv.x * x_scale + (1.0 - x_scale) * 0.5;
	o.uv.y = v.uv.y * y_scale + (1.0 - y_scale) * 0.5;
	return o;
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

float4 fragment(v2f i) : SV_Target {
	return _MainTex.Sample(linear_clamp_sampler, i.uv);
}

#endif
