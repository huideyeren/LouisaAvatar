#ifndef VIRTUALLENS2_LOGIBOKEH_REALTIME_SMAA_BLENDING_WEIGHT_CALCULATION_CGINC
#define VIRTUALLENS2_LOGIBOKEH_REALTIME_SMAA_BLENDING_WEIGHT_CALCULATION_CGINC

#include "UnityCG.cginc"
#include "../../Common/TargetDetector.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

Texture2D _GrabTexture;
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

	if(!isVirtualLensComputeCamera()){
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
	return SMAABlendingWeightCalculationPS(
		i.uv, i.pixcoord, i.offset, _GrabTexture, _SMAAAreaTex, _SMAASearchTex, 0);
}

#endif
