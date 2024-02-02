#ifndef VIRTUALLENS2_COMMON_DEPTH_CGINC
#define VIRTUALLENS2_COMMON_DEPTH_CGINC

// Compute depth in meter
float linearizeDepth(float d, float near, float far){
	const float z = 1.0 / far - 1.0 / near;
	const float w = 1.0 / near;
	return 1.0 / (z * (1.0 - d) + w);
}

#endif
