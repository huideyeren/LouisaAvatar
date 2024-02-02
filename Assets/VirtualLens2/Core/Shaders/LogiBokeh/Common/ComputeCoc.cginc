#ifndef VIRTUALLENS2_LOGIBOKEH_COMMON_COMPUTE_COC_CGINC
#define VIRTUALLENS2_LOGIBOKEH_COMMON_COMPUTE_COC_CGINC

#include "../../Common/Constants.cginc"
#include "../../Common/StateTexture.cginc"
#include "../../Common/FieldOfView.cginc"
#include "../../Common/MultiSamplingHelper.cginc"
#include "Configuration.cginc"
#include "Samplers.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

float _Near;              // Distance to near plane
float _Far;               // Distance to far plane
float _FieldOfView;       // Raw field of view [deg]
float _LogFNumber;        // log(F)
float _BlurringThresh;    // Disable DoF simulation when _LogFNumber >= _BlurringThresh
int   _MaxNumRings;       // Maximum number of scatter-as-gather rings


//---------------------------------------------------------------------------
// Utility functions
//---------------------------------------------------------------------------

float linearizeDepth(float d){
	const float z = 1.0 / _Far - 1.0 / _Near;
	const float w = 1.0 / _Near;
	return 1.0 / (z * (1.0 - d) + w);
}


float3 computeCocParameters(TEXTURE2DMS<float> depthTex, float2 resolution){
	int width, height, num_samples;
	TEXTURE2DMS_GET_DIMENSIONS(depthTex, width, height, num_samples);

	const float2 focus_point = getFocusPointState();
	const int2   focus_coord = (int2)floor(focus_point * float2(width, height));

	const float f = computeFocalLength(_FieldOfView);
	const float a = f / exp(_LogFNumber);
	const float p = getFocusDistance();

	// Max CoC RADIUS in HALF resolution
	const float max_coc = ((a * f) / (p - f)) * (resolution.x / SENSOR_SIZE) * 0.25;
	return float3(max_coc, p, ring_border_radius(_MaxNumRings));
}


float foregroundAlphaCompensation(float coc){
	const float threshold = -1.0;
	if(coc >= threshold){
		return coc;
	}else{
		return threshold + (coc - threshold) * rcp(FOREGROUND_ALPHA_MULTIPLE);
	}
}

float computeCoc(float d, float3 params){
	const float depth     = linearizeDepth(d);
	const float max_coc   = params.x;
	const float p         = params.y;
	const float coc_limit = params.z;
	const float coc       = (1.0 - p / depth) * max_coc;
	return clamp(foregroundAlphaCompensation(coc), -coc_limit, coc_limit);
}


#endif
