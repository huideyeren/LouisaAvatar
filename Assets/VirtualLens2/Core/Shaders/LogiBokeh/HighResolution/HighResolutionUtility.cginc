#ifndef VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_HIGH_RESOLUTION_UTILITY_CGINC
#define VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_HIGH_RESOLUTION_UTILITY_CGINC

//---------------------------------------------------------------------------
// Utility functions
//---------------------------------------------------------------------------

float2 computePaddedUV(float2 uv){
	const float2 scale  = float2(3840.0 / 4096.0, 2160.0 / 2304.0);
	const float2 offset = 0.5 - 0.5 * scale;
	return uv * scale + offset;
}

float2 computeUnpaddedUV(float2 uv){
	const float2 scale  = float2(4096.0 / 3840.0, 2304.0 / 2160.0);
	const float2 offset = 0.5 - 0.5 * scale;
	return uv * scale + offset;
}


#endif
