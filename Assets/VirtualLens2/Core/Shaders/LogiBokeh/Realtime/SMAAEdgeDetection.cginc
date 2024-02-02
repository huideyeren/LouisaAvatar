#ifndef VIRTUALLENS2_LOGIBOKEH_REALTIME_SMAA_EDGE_DETECTION_CGINC
#define VIRTUALLENS2_LOGIBOKEH_REALTIME_SMAA_EDGE_DETECTION_CGINC

#include "UnityCG.cginc"
#include "../../Common/TargetDetector.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

Texture2D _VirtualLens2_BlurredTex;


//---------------------------------------------------------------------------
// Structure definitions
//---------------------------------------------------------------------------

struct v2f {
	float4 vertex    : SV_POSITION;
	float2 uv        : TEXCOORD0;
	float4 offset[3] : TEXCOORD1;
};


//---------------------------------------------------------------------------
// Vertex shader
//---------------------------------------------------------------------------

v2f vertex(appdata v){
	v2f o;

	if(!isVirtualLensComputeCamera()){
		o.vertex    = 0;
		o.uv        = 0;
		o.offset[0] = 0;
		o.offset[1] = 0;
		o.offset[2] = 0;
		return o;
	}

	o.vertex = computeVertexPosition(v);
	o.uv     = computeVertexTexCoord(v);
	SMAAEdgeDetectionVS(o.uv, o.offset);
	return o;
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

float4 fragment(v2f i) : SV_Target {
	return float4(SMAALumaEdgeDetectionPS(i.uv, i.offset, _VirtualLens2_BlurredTex), 0, 0);
}

#endif
