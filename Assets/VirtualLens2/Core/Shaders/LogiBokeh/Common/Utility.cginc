#ifndef VIRTUALLENS2_LOGIBOKEH_COMMON_UTILITY_CGINC
#define VIRTUALLENS2_LOGIBOKEH_COMMON_UTILITY_CGINC

#include "../../Common/Constants.cginc"


//---------------------------------------------------------------------------
// Utility functions
//---------------------------------------------------------------------------

float sample_alpha(float coc){
	return saturate(rcp(PI * coc * coc));
}


float4 alpha_blend(float4 dst, float4 src){
	const float out_a = src.a + dst.a * (1.0 - src.a);
	if(out_a == 0.0){ return 0.0; }
	const float3 out_rgb = (src.rgb * src.a + dst.rgb * dst.a * (1.0 - src.a)) / out_a;
	return float4(out_rgb, out_a);
}


float4 premultiplied_alpha_blend(float4 dst, float4 src){
	const float out_a = src.a + dst.a * (1.0 - src.a);
	if(out_a == 0.0){ return 0.0; }
	const float3 out_rgb = (src.rgb + dst.rgb * dst.a * (1.0 - src.a)) / out_a;
	return float4(out_rgb, out_a);
}


float rgb2luma(float3 rgb){
	return rgb.r * 0.299 + rgb.g * 0.587 + rgb.b * 0.114;
}


float gamma2linear(float x){
	static const float a = 0.055;
	if(x <= 0.0405){
		return x / 12.92;
	}else{
		return pow((x + a) / (1.0 + a), 2.4);
	}
}

float3 gamma2linear(float3 x){
	return float3(gamma2linear(x.r), gamma2linear(x.g), gamma2linear(x.b));
}

float linear2gamma(float x){
	static const float a = 0.055;
	if(x <= 0.0031308){
		return x * 12.92;
	}else{
		return (1.0 + a) * pow(x, 1.0 / 2.4) - a;
	}
}

float3 linear2gamma(float3 x){
	return float3(linear2gamma(x.r), linear2gamma(x.g), linear2gamma(x.b));
}


#endif
