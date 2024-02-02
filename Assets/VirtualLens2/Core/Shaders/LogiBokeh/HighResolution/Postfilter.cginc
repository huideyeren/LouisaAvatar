#ifndef VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_POSTFILTER_CGINC
#define VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_POSTFILTER_CGINC

#include "UnityCG.cginc"
#include "../../Common/Constants.cginc"
#include "../../Common/TargetDetector.cginc"

#include "HighResolutionColorConfig.cginc"
#include "../Common/Postfilter.cginc"
#include "HighResolutionUtility.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

float _Blurring;


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

	if(_Blurring == 0.0 || !isVRChatCamera4K()){
		o.vertex = 0;
		o.uv     = 0;
		return o;
	}

	const float x = v.uv.x;
	const float y = v.uv.y * 0.5;

	const float proj_x = (x * 2 - 1);
	const float proj_y = (y * 2 - 1) * _ProjectionParams.x;
#ifdef UNITY_REVERSED_Z
	const float proj_z = 1.0;
#else
	const float proj_z = UNITY_NEAR_CLIP_VALUE;
#endif
	o.vertex = float4(proj_x, proj_y, proj_z, 1);
	o.uv     = float2(x, y);
	return o;
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

float4 fragment(v2f i) : SV_Target {
	if(i.uv.x < 0.5){
		const float2 uv = i.uv * float2(2.0, 2.0) - float2(0.0, 0.0);
		return backgroundPostfilter(computePaddedUV(uv), i.uv);
	}else{
		const float2 uv = i.uv * float2(2.0, 2.0) - float2(1.0, 0.0);
		return foregroundPostfilter(computePaddedUV(uv), i.uv);
	}
}


#endif
