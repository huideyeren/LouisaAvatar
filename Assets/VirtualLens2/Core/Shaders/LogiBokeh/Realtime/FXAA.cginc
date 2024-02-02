#ifndef VIRTUALLENS2_LOGI_BOKEH_REALTIME_FXAA_HLSL
#define VIRTUALLENS2_LOGI_BOKEH_REALTIME_FXAA_HLSL

#include "UnityCG.cginc"
#include "../../Common/Constants.cginc"
#include "../../Common/TargetDetector.cginc"

#define FXAA_PC              (1)
#define FXAA_HLSL_5          (1)
#define FXAA_GREEN_AS_LUMA   (0)
#define FXAA_QUALITY__PRESET (13)

#include "../../FXAA/FXAA.hlsl"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

Texture2D _GrabTexture;
float4 _GrabTexture_TexelSize;

SamplerState linear_clamp_sampler;


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

	if(!isVirtualLensComputeCamera()){
		o.vertex = 0;
		o.uv     = 0;
		return o;
	}

	const float x = v.uv.x;
	const float y = v.uv.y;

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
	FxaaTex input = { linear_clamp_sampler, _GrabTexture };
    const float3 rgb = FxaaPixelShader(
		i.uv,                                // FxaaFloat2 pos,
		FxaaFloat4(0.0f, 0.0f, 0.0f, 0.0f),  // FxaaFloat4 fxaaConsolePosPos,
		input,                               // FxaaTex tex,
		input,                               // FxaaTex fxaaConsole360TexExpBiasNegOne,
		input,                               // FxaaTex fxaaConsole360TexExpBiasNegTwo,
		abs(_GrabTexture_TexelSize.xy),      // FxaaFloat2 fxaaQualityRcpFrame,
		FxaaFloat4(0.0f, 0.0f, 0.0f, 0.0f),  // FxaaFloat4 fxaaConsoleRcpFrameOpt,
		FxaaFloat4(0.0f, 0.0f, 0.0f, 0.0f),  // FxaaFloat4 fxaaConsoleRcpFrameOpt2,
		FxaaFloat4(0.0f, 0.0f, 0.0f, 0.0f),  // FxaaFloat4 fxaaConsole360RcpFrameOpt2,
		0.75f,                               // FxaaFloat fxaaQualitySubpix,
		0.166f,                              // FxaaFloat fxaaQualityEdgeThreshold,
		0.0833f,                             // FxaaFloat fxaaQualityEdgeThresholdMin,
		0.0f,                                // FxaaFloat fxaaConsoleEdgeSharpness,
		0.0f,                                // FxaaFloat fxaaConsoleEdgeThreshold,
		0.0f,                                // FxaaFloat fxaaConsoleEdgeThresholdMin,
		FxaaFloat4(0.0f, 0.0f, 0.0f, 0.0f)   // FxaaFloat fxaaConsole360ConstDir,
	);
	return float4(rgb, 1.0);
}

#endif
