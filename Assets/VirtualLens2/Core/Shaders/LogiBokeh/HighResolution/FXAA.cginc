#ifndef VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_FXAA_HLSL
#define VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_FXAA_HLSL

#include "UnityCG.cginc"
#include "../../Common/Constants.cginc"
#include "../../Common/TargetDetector.cginc"

#define FXAA_PC              (1)
#define FXAA_HLSL_5          (1)
#define FXAA_GREEN_AS_LUMA   (0)

#if defined(REALTIME_FXAA)
#define FXAA_QUALITY__PRESET (13)
#elif defined(HIGH_RESOLUTION_FXAA)
#define FXAA_QUALITY__PRESET (39)
#endif

#include "../../FXAA/FXAA.hlsl"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

#if defined(REALTIME_FXAA)
Texture2D _GrabTexture;
float4 _GrabTexture_TexelSize;
#elif defined(HIGH_RESOLUTION_FXAA)
Texture2D _VirtualLens2_BlurredTex;
float4 _VirtualLens2_BlurredTex_TexelSize;
#endif

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

#if defined(REALTIME_FXAA)
	if(!isVirtualLensComputeCamera()){
#elif defined(HIGH_RESOLUTION_FXAA)
	if(!isVRChatCamera4K()){
#else
	{
#endif
		o.vertex = 0;
		o.uv     = 0;
		return o;
	}

	const float x = v.uv.x;
	const float y = v.uv.y;

	const float proj_x = (x * 2 - 1);
	const float proj_y = (y * 2 - 1) * _ProjectionParams.x;
#ifdef UNITY_REVERSED_Z
	const float proj_z = 1.0;
#else
	const float proj_z = UNITY_NEAR_CLIP_VALUE;
#endif
	o.vertex = float4(proj_x, proj_y, proj_z, 1);
	o.uv     = v.uv;
	return o;
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

float4 fragment(v2f i) : SV_Target {
#if defined(REALTIME_FXAA)
	FxaaTex input = { linear_clamp_sampler, _GrabTexture };
	const float2 texel_size = abs(_GrabTexture_TexelSize.xy);
#elif defined(HIGH_RESOLUTION_FXAA)
	FxaaTex input = { linear_clamp_sampler, _VirtualLens2_BlurredTex };
	const float2 texel_size = abs(_VirtualLens2_BlurredTex_TexelSize.xy);
#endif
    const float3 rgb = FxaaPixelShader(
		i.uv,                                // FxaaFloat2 pos,
		FxaaFloat4(0.0f, 0.0f, 0.0f, 0.0f),  // FxaaFloat4 fxaaConsolePosPos,
		input,                               // FxaaTex tex,
		input,                               // FxaaTex fxaaConsole360TexExpBiasNegOne,
		input,                               // FxaaTex fxaaConsole360TexExpBiasNegTwo,
		texel_size,                          // FxaaFloat2 fxaaQualityRcpFrame,
		FxaaFloat4(0.0f, 0.0f, 0.0f, 0.0f),  // FxaaFloat4 fxaaConsoleRcpFrameOpt,
		FxaaFloat4(0.0f, 0.0f, 0.0f, 0.0f),  // FxaaFloat4 fxaaConsoleRcpFrameOpt2,
		FxaaFloat4(0.0f, 0.0f, 0.0f, 0.0f),  // FxaaFloat4 fxaaConsole360RcpFrameOpt2,
		0.75f,                               // FxaaFloat fxaaQualitySubpix,
		0.1f,                                // FxaaFloat fxaaQualityEdgeThreshold,
		0.05f,                               // FxaaFloat fxaaQualityEdgeThresholdMin,
		0.0f,                                // FxaaFloat fxaaConsoleEdgeSharpness,
		0.0f,                                // FxaaFloat fxaaConsoleEdgeThreshold,
		0.0f,                                // FxaaFloat fxaaConsoleEdgeThresholdMin,
		FxaaFloat4(0.0f, 0.0f, 0.0f, 0.0f)   // FxaaFloat fxaaConsole360ConstDir,
	);
	return float4(rgb, 1.0);
}

#endif
