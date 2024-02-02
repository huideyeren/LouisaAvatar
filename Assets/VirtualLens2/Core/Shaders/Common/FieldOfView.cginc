#ifndef VIRTUALLENS2_COMMON_FIELD_OF_VIEW_CGINC
#define VIRTUALLENS2_COMMON_FIELD_OF_VIEW_CGINC

#include "Constants.cginc"

// Convert enlarged field of view [deg] to focal length [m]
float computeFocalLength(float enlarged_fov){
	const float DEG2RAD = 0.01745329251;
	const float SCALE = SCREEN_ENLARGEMENT.y;
	return (0.010125 * SCALE) / tan(0.5 * DEG2RAD * enlarged_fov);
}

#endif
