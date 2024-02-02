#ifndef VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_SMAA_BLENDING_WEIGHT_CALCULATION_CGINC
#define VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_SMAA_BLENDING_WEIGHT_CALCULATION_CGINC

#include "UnityCG.cginc"
#include "../../Common/TargetDetector.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

#if defined(REALTIME_SMAA)
Texture2D _GrabTexture;
#elif defined(HIGH_RESOLUTION_SMAA)
Texture2D _VirtualLens2_SMAAEdgeTex;
#endif

Texture2D _SMAAAreaTex;
Texture2D _SMAASearchTex;


//---------------------------------------------------------------------------
// Structure definitions
//---------------------------------------------------------------------------

struct v2f {
	float4 vertex    : SV_POSITION;
	float2 uv        : TEXCOORD0;
	float2 pixcoord  : TEXCOORD1;
	float4 offset[3] : TEXCOORD2;
};


//---------------------------------------------------------------------------
// Vertex shader
//---------------------------------------------------------------------------

v2f vertex(appdata v){
	v2f o;

#if defined(REALTIME_SMAA)
	if(!isVirtualLensComputeCamera()){
#elif defined(HIGH_RESOLUTION_SMAA)
	if(!isVRChatCamera4K()){
#else
	{
#endif
		o.vertex    = 0;
		o.uv        = 0;
		o.pixcoord  = 0;
		o.offset[0] = 0;
		o.offset[1] = 0;
		o.offset[2] = 0;
		return o;
	}

	o.vertex = computeVertexPosition(v);
	o.uv     = computeVertexTexCoord(v);
	SMAABlendingWeightCalculationVS(o.uv, o.pixcoord, o.offset);
	return o;
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

float4 fragment(v2f i) : SV_Target {
#if defined(REALTIME_SMAA)
	return SMAABlendingWeightCalculationPS(
		i.uv, i.pixcoord, i.offset, _GrabTexture, _SMAAAreaTex, _SMAASearchTex, 0);
#elif defined(HIGH_RESOLUTION_SMAA)
	return SMAABlendingWeightCalculationPS(
		i.uv, i.pixcoord, i.offset, _VirtualLens2_SMAAEdgeTex, _SMAAAreaTex, _SMAASearchTex, 0);
#endif
}

#endif
