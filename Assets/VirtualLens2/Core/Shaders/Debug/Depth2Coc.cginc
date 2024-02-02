#ifndef VIRTUALLENS2_DEBUG_DEPTH2COC_CGINC
#define VIRTUALLENS2_DEBUG_DEPTH2COC_CGINC

#include "UnityCustomRenderTexture.cginc"
#include "../Common/Constants.cginc"
#include "../Common/FieldOfView.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

float _FieldOfView;    // Raw field of view [deg]
float _FNumber;        // F-Number
float _FocusDistance;  // Distance to focus plane [m]

Texture2D _MainTex;

SamplerState point_clamp_sampler;


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

float4 sample(float2 uv){
	return _MainTex.Sample(point_clamp_sampler, uv);
}

float4 fragment(v2f_customrendertexture i) : SV_Target {
	const float f = computeFocalLength(_FieldOfView);
	const float a = f / _FNumber;
	const float p = _FocusDistance;
	// Maximum CoC in pixels when screen size equals to (1920, 1080)
	const float max_coc = ((a * f) / (p - f)) * (1920.0 / SENSOR_SIZE);

	const float4 c   = sample(i.globalTexcoord);
	const float  z   = c.a;
	const float  coc = (1 - p / z) * max_coc;
	return float4(c.rgb, coc);
}

#endif
