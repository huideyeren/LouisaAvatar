#ifndef VIRTURLLENS2_SYSTEM_UNLIT_PREVIEW_CGINC
#define VIRTURLLENS2_SYSTEM_UNLIT_PREVIEW_CGINC

#include "UnityCG.cginc"
#include "../Common/Constants.cginc"
#include "../Common/TargetDetector.cginc"
#include "../Common/StateTexture.cginc"
#include "../Common/FieldOfView.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

sampler2D _MainTex;  // Rendered texture


//---------------------------------------------------------------------------
// Structure definitions
//---------------------------------------------------------------------------

struct appdata {
	float4 vertex : POSITION;
	float2 uv     : TEXCOORD0;

	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f {
	float4 vertex : SV_POSITION;
	float2 uv     : TEXCOORD0;

	UNITY_VERTEX_OUTPUT_STEREO
};


//---------------------------------------------------------------------------
// Vertex shader
//---------------------------------------------------------------------------

v2f vertex(appdata v){
	v2f o;

	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_OUTPUT(v2f, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	if(isVirtualLensCaptureCamera()){
		o.vertex = 0;
		o.uv     = 0;
		return o;
	}

	o.vertex = UnityObjectToClipPos(v.vertex);
	o.uv     = v.uv;
	return o;
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

float4 fragment(v2f i) : SV_Target {
	return float4(tex2D(_MainTex, i.uv).rgb, 0.5);
}

#endif
