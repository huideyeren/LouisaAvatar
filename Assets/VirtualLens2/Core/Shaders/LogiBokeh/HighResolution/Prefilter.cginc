#ifndef VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_PREFILTER_COC_CGINC
#define VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_PREFILTER_COC_CGINC

#include "UnityCG.cginc"
#include "../../Common/Constants.cginc"
#include "../../Common/TargetDetector.cginc"
#include "../../Common/MultiSamplingHelper.cginc"

#include "HighResolutionCocConfig.cginc"
#include "HighResolutionColorConfig.cginc"
#include "HighResolutionTileConfig.cginc"
#include "RingKernel.cginc"
#include "../Common/PremultiplyFull.cginc"
#include "../Common/BackgroundPrefilter.cginc"
#include "../Common/ForegroundPrefilter.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

float _Blurring;

TEXTURE2DMS<float3> _MainTex;
TEXTURE2DMS<float>  _DepthTex;


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
	float3 params : TEXCOORD1;
};


//---------------------------------------------------------------------------
// Vertex shader
//---------------------------------------------------------------------------

v2f vertex(appdata v){
	v2f o;

	if(_Blurring == 0.0 || !isVirtualLensCustomComputeCamera(4096, 4608)){
		o.vertex = 0;
		o.uv     = 0;
		o.params = 0;
		return o;
	}

	const float x = v.uv.x;
	const float y = v.uv.y * 0.75;

	const float proj_x = (x * 2 - 1);
	const float proj_y = (y * 2 - 1) * _ProjectionParams.x;
	o.vertex = float4(proj_x, proj_y, 0, 1);
	o.uv     = float2(x, y);
	o.params = computeCocParameters(_DepthTex, float2(3840.0, 2160.0));
	return o;
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

float4 fragment(v2f i) : SV_Target {
	if(i.uv.y < 0.5){
		const float2 uv = i.uv * float2(1.0, 2.0) - float2(0.0, 0.0);
		return premultiplyFull(_MainTex, _DepthTex, uv, i.params);
	}else if(i.uv.x < 0.5){
		const float2 uv = i.uv * float2(2.0, 4.0) - float2(0.0, 2.0);
		return downsampleBackground(_MainTex, _DepthTex, uv, i.params);
	}else{
		const float2 uv = i.uv * float2(2.0, 4.0) - float2(1.0, 2.0);
		return downsampleForeground(_MainTex, _DepthTex, uv, i.params);
	}
}


#endif
