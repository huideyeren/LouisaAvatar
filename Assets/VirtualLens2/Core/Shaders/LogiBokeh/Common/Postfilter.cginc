#ifndef VIRTUALLENS2_LOGIBOKEH_COMMON_POST_FILTER_CGINC
#define VIRTUALLENS2_LOGIBOKEH_COMMON_POST_FILTER_CGINC

#include "Samplers.cginc"

#define FXAA_PC              (1)
#define FXAA_HLSL_5          (1)
#define FXAA_GREEN_AS_LUMA   (0)
#define FXAA_QUALITY__PRESET (13)
#include "PostfilterFXAA.cginc"


//---------------------------------------------------------------------------
// Utility functions
//---------------------------------------------------------------------------

float4 backgroundPostfilter(float2 uv, float2 raw_uv){
	const float2 texel_size = WORKAREA_TEXEL_SIZE;
	// FXAA
	FxaaTex input = { linear_clamp_sampler, rawWorkareaTexture() };
	const float2 raw_center = FxaaPixelShader(
		raw_uv,                              // FxaaFloat2 pos,
		FxaaFloat4(0.0f, 0.0f, 0.0f, 0.0f),  // FxaaFloat4 fxaaConsolePosPos,
		input,                               // FxaaTex tex,
		input,                               // FxaaTex fxaaConsole360TexExpBiasNegOne,
		input,                               // FxaaTex fxaaConsole360TexExpBiasNegTwo,
		rawWorkareaTexelSize(),              // FxaaFloat2 fxaaQualityRcpFrame,
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
	const float2 raw_offset = raw_center - raw_uv;
	const float2 offset = raw_offset * (texel_size / rawWorkareaTexelSize());
	const float2 center = uv + offset;
	// Tent filter
	const float4 sum =
		  fetchBackgroundWorkarea(linear_clamp_sampler, center + texel_size * float2(-0.5, -0.5))
		+ fetchBackgroundWorkarea(linear_clamp_sampler, center + texel_size * float2( 0.5, -0.5))
		+ fetchBackgroundWorkarea(linear_clamp_sampler, center + texel_size * float2(-0.5,  0.5))
		+ fetchBackgroundWorkarea(linear_clamp_sampler, center + texel_size * float2( 0.5,  0.5));
	return sum * 0.25;
}


float4 foregroundPostfilter(float2 uv, float2 raw_uv){
	const float2 texel_size = WORKAREA_TEXEL_SIZE;
	// FXAA
	FxaaTex input = { linear_clamp_sampler, rawWorkareaTexture() };
    const float2 raw_center = FxaaPixelShader(
		raw_uv,                              // FxaaFloat2 pos,
		FxaaFloat4(0.0f, 0.0f, 0.0f, 0.0f),  // FxaaFloat4 fxaaConsolePosPos,
		input,                               // FxaaTex tex,
		input,                               // FxaaTex fxaaConsole360TexExpBiasNegOne,
		input,                               // FxaaTex fxaaConsole360TexExpBiasNegTwo,
		abs(rawWorkareaTexelSize()),         // FxaaFloat2 fxaaQualityRcpFrame,
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
	const float2 raw_offset = raw_center - raw_uv;
	const float2 offset = raw_offset * (texel_size / rawWorkareaTexelSize());
	const float2 center = uv + offset;
	// Tent filter
	const float4 sum =
		  fetchForegroundWorkarea(linear_clamp_sampler, center + texel_size * float2(-0.5, -0.5))
		+ fetchForegroundWorkarea(linear_clamp_sampler, center + texel_size * float2( 0.5, -0.5))
		+ fetchForegroundWorkarea(linear_clamp_sampler, center + texel_size * float2(-0.5,  0.5))
		+ fetchForegroundWorkarea(linear_clamp_sampler, center + texel_size * float2( 0.5,  0.5));
	return sum * 0.25;
}

#endif
