#ifndef NN4VRC_YOLOX_TRACKING_PARAMETERS_CGINC
#define NN4VRC_YOLOX_TRACKING_PARAMETERS_CGINC

#include "../../Common/StateTexture.cginc"

float _TrackingSpeed;

float get_face_tracking_distance_limit(){
	const float p_dt = _Time.y - getLastPixelTrackedTimestamp();
	if(p_dt >= 1.0){ return 1.#INF; }
	const float f_dt = _Time.y - getLastFaceTrackedTimestamp();
	return max(0.15, max(f_dt * 0.1, p_dt) * _TrackingSpeed);
}

float get_pixel_tracking_distance_limit(){
	const float dt = _Time.y - getLastPixelTrackedTimestamp();
	return max(0.1, dt * _TrackingSpeed);
}

#endif
