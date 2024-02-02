#ifndef VIRTUALLENS2_LOGIBOKEH_REALTIME_COMPOSITION_CGINC
#define VIRTUALLENS2_LOGIBOKEH_REALTIME_COMPOSITION_CGINC

#include "UnityCG.cginc"
#include "../../Common/Constants.cginc"
#include "../../Common/TargetDetector.cginc"
#include "../../Common/MultiSamplingHelper.cginc"

#include "RealtimeCocConfig.cginc"
#include "RealtimeTileConfig.cginc"
#include "RealtimeColorConfig.cginc"
#include "RingKernel.cginc"
#include "../Common/Composition.cginc"
#include "RealtimeUtility.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

float _Blurring;
float _Exposure;
int _MaxNumRings;

TEXTURE2DMS<float3> _MainTex;


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
	float2 params : TEXCOORD1;  // (linear exposure, reserved)
};


//---------------------------------------------------------------------------
// Vertex shader
//---------------------------------------------------------------------------

v2f vertex(appdata v){
	v2f o;

	if(!isVirtualLensComputeCamera()){
		o.vertex = 0;
		o.uv     = 0;
		o.params = 0;
		return o;
	}

	const float x = v.uv.x;
	const float y = v.uv.y;

	const float proj_x = (x * 2 - 1);
	const float proj_y = (y * 2 - 1) * _ProjectionParams.x;
	o.vertex = float4(proj_x, proj_y, 0, 1);
	o.uv     = float2(x, y);
	o.params = float2(exp(_Exposure), 0);
	return o;
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

float4 fragment(v2f i) : SV_Target {
	if(!inRequiredRange(i.uv)){ discard; }
	const float exposure = i.params.x;
	const float3 rgb = (_Blurring > 0.0)
		? exposure * composition(_MaxNumRings, i.uv)
		: exposure * passthrough(_MainTex, i.uv);
	return prepareAntiAliasing(rgb);
}


#endif
