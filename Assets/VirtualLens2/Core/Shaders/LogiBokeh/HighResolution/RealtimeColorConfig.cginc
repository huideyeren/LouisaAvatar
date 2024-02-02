#ifndef VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_HIGH_RESOLUTION_COLOR_CONFIG_CGINC
#define VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_HIGH_RESOLUTION_COLOR_CONFIG_CGINC

/* _DownsampledTex layout
 *
 *   +---------------+---------------+
 *   |               |               |
 *   |    RT Full    |    RT Full    |
 *   |               |               |
 *   +---------------+---------------+
 *   |               |               |
 *   |  Background   |  Foreground   |
 *   |               |               |
 *   +---------------+---------------+
 *   |                               |
 *   |                               |
 *   |                               |
 *   |            HR Full            |
 *   |                               |
 *   |                               |
 *   |                               |
 *   +-------------------------------+
 *
 *  - RT Full:    2048 x 1152
 *  - RT Full:    2048 x 1152 (Premultiplied)
 *  - Background: 2048 x 1152
 *  - Foreground: 2048 x 1152
 *  - HR Full:    4096 x 2304 (Premultiplied)
 */

static const float2 FULL_TEXEL_SIZE = float2(1.0 / 2048.0, 1.0 / 1152.0);
static const float2 PREMULTIPLIED_FULL_TEXEL_SIZE = float2(1.0 / 2048.0, 1.0 / 1152.0);

Texture2D _DownsampledTex;

float4 fetchFullSample(SamplerState s, float2 uv){
	const float2 minval = float2(0.0, 0.75);
	const float2 maxval = float2(0.5, 1.0);
	const float2 scale  = maxval - minval;
	const float2 offset = minval;
	return _DownsampledTex.SampleLevel(s, uv * scale + offset, 0);
}

float4 fetchPremultipliedFullSample(SamplerState s, float2 uv){
	const float2 minval = float2(0.5, 0.75);
	const float2 maxval = float2(1.0, 1.0);
	const float2 scale  = maxval - minval;
	const float2 offset = minval;
	return _DownsampledTex.SampleLevel(s, uv * scale + offset, 0);
}


/* _GrabTexture layout
 *
 *   +-------------------------------+
 *   |                               |
 *   |                               |
 *   |                               |
 *   +---------------+---------------+
 *   |               |               |
 *   |  Background   |  Foreground   |
 *   |               |               |
 *   +---------------+---------------+
 *
 *  - Background: 1920 x 1080 
 *  - Foreground: 1920 x 1080
 */

static const float2 DOWNSAMPLED_TEXEL_SIZE = float2(1.0 / 1024.0, 1.0 / 576.0);
static const float2 WORKAREA_TEXEL_SIZE    = float2(1.0 / 1024.0, 1.0 / 576.0);

Texture2D _GrabTexture;


float4 fetchDownsampledBackground(SamplerState s, float2 uv){
	const float2 texel_size = DOWNSAMPLED_TEXEL_SIZE;
	uv = clamp(uv, 0.5 * texel_size, 1.0 - 0.5 * texel_size);
	const float2 minval = float2(0.0, 0.0);
	const float2 maxval = float2(0.5, 0.5);
	const float2 scale  = maxval - minval;
	const float2 offset = minval;
	return _GrabTexture.SampleLevel(s, uv * scale + offset, 0);
}

float4 fetchDownsampledForeground(SamplerState s, float2 uv){
	const float2 texel_size = DOWNSAMPLED_TEXEL_SIZE;
	uv = clamp(uv, 0.5 * texel_size, 1.0 - 0.5 * texel_size);
	const float2 minval = float2(0.5, 0.0);
	const float2 maxval = float2(1.0, 0.5);
	const float2 scale  = maxval - minval;
	const float2 offset = minval;
	return _GrabTexture.SampleLevel(s, uv * scale + offset, 0);
}


Texture2D rawWorkareaTexture(){
	return _GrabTexture;
}

float2 rawWorkareaTexelSize(){
	return float2(1.0 / 2048.0, 1.0 / 1152.0);
}


float4 fetchBackgroundWorkarea(SamplerState s, float2 uv){
	const float2 texel_size = WORKAREA_TEXEL_SIZE;
	uv = clamp(uv, 0.5 * texel_size, 1.0 - 0.5 * texel_size);
	const float2 minval = float2(0.0, 0.0);
	const float2 maxval = float2(0.5, 0.5);
	const float2 scale  = maxval - minval;
	const float2 offset = minval;
	return _GrabTexture.SampleLevel(s, uv * scale + offset, 0);
}

float4 fetchForegroundWorkarea(SamplerState s, float2 uv){
	const float2 texel_size = WORKAREA_TEXEL_SIZE;
	uv = clamp(uv, 0.5 * texel_size, 1.0 - 0.5 * texel_size);
	const float2 minval = float2(0.5, 0.0);
	const float2 maxval = float2(1.0, 0.5);
	const float2 scale  = maxval - minval;
	const float2 offset = minval;
	return _GrabTexture.SampleLevel(s, uv * scale + offset, 0);
}


#endif
