#ifndef VIRTUALLENS2_LOGIBOKEH_REALTIME_SMAA_NEIGHBORHOOD_BLENDING_CGINC
#define VIRTUALLENS2_LOGIBOKEH_REALTIME_SMAA_NEIGHBORHOOD_BLENDING_CGINC

#include "UnityCG.cginc"
#include "../../Common/TargetDetector.cginc"
#include "../Common/Utility.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

Texture2D _VirtualLens2_BlurredTex;
Texture2D _GrabTexture;


//---------------------------------------------------------------------------
// Structure definitions
//---------------------------------------------------------------------------

struct v2f {
	float4 vertex : SV_POSITION;
	float2 uv     : TEXCOORD0;
	float4 offset : TEXCOORD1;
};


//---------------------------------------------------------------------------
// Vertex shader
//---------------------------------------------------------------------------

v2f vertex(appdata v){
	v2f o;

	if(!isVirtualLensComputeCamera()){
		o.vertex = 0;
		o.uv     = 0;
		o.offset = 0;
		return o;
	}

	o.vertex = computeVertexPosition(v);
	o.uv     = computeVertexTexCoord(v);
	SMAANeighborhoodBlendingVS(o.uv, o.offset);
	return o;
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

float4 fragment(v2f i) : SV_Target {
	const float3 rgb = SMAANeighborhoodBlendingPS(i.uv, i.offset, _VirtualLens2_BlurredTex, _GrabTexture);
	return float4(gamma2linear(rgb), 1.0);
}

#endif
