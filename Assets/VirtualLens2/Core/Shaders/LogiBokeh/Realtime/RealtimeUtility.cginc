#ifndef VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_REALTIME_UTILITY_CGINC
#define VIRTUALLENS2_LOGIBOKEH_HIGH_RESOLUTION_REALTIME_UTILITY_CGINC

//---------------------------------------------------------------------------
// Utility functions
//---------------------------------------------------------------------------

bool inRequiredRange(float2 uv){
	const float2 scale   = float2(1952.0 / 2048.0, 1112.0 / 1152.0);
	const float2 padding = 0.5 - 0.5 * scale;
	if(uv.x < padding.x || 1.0 - padding.x < uv.x){ return false; }
	if(uv.y < padding.y || 1.0 - padding.y < uv.y){ return false; }
	return true;
}

#endif
