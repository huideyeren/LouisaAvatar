#ifndef VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_DILATE_TILES_CGINC
#define VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_DILATE_TILES_CGINC

#include "UnityCG.cginc"
#include "../../Common/Constants.cginc"
#include "../../Common/TargetDetector.cginc"

#include "RealtimeCocConfig.cginc"
#include "RingKernel.cginc"
#include "../Common/TileDilator.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

float _Blurring;
int _MaxNumRings;


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

	if(_Blurring == 0.0 || !isVirtualLensCustomComputeCamera(256, 360)){
		o.vertex = 0;
		o.uv     = 0;
		return o;
	}

	const float x = v.uv.x * 0.5 + 0.5;
	const float y = v.uv.y * (1.0 / 5.0) + (4.0 / 5.0);

	const float proj_x = (x * 2 - 1);
	const float proj_y = (y * 2 - 1) * _ProjectionParams.x;
	o.vertex = float4(proj_x, proj_y, 0, 1);
	o.uv     = v.uv;
	return o;
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

float4 fragment(v2f i) : SV_Target {
	const float2 region_min = float2(0.0, 4.0 / 5.0);
	const float2 region_max = float2(0.5, 1.0);
	return dilateTile(_MaxNumRings, i.uv, region_min, region_max);
}


#endif
