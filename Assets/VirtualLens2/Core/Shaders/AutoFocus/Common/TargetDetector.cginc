#ifndef NN4VRC_TARGET_DETECTOR_CGINC
#define NN4VRC_TARGET_DETECTOR_CGINC

#include "UnityCG.cginc"

// Mirror
bool testInMirror(){
	// https://discord.com/channels/189511567539306508/449386885299830794/589283937516519446
	return unity_CameraProjection._m20 != 0.0
		|| unity_CameraProjection._m21 != 0.0;
}

// Stereo (VR HMD)
bool testStereo(){
#if UNITY_SINGLE_PASS_STEREO
	return true;
#else
	return false;
#endif
}

// Perspective or orthogonal
bool testPerspective(){
	return unity_OrthoParams.w == 0.0;
}


// Compute camera
//   - Orthogonal (size = 0.125)
//   - (near, far) = (0, 0.25)
//   - Monocular
//   - Not in mirror
bool isComputeCamera(int width, int height){
	return _ScreenParams.x == width      // Screen width
		&& _ScreenParams.y == height     // Screen height
		&& !testPerspective()            // Orthogonal
		&& unity_OrthoParams.y == 0.125  // Orthographic camera's height
		&& _ProjectionParams.y == 0.0    // Near plane
		&& _ProjectionParams.z == 0.25   // Far plane
		&& !testStereo()                 // Monocular
		&& !testInMirror();              // Not in mirror
}

#endif
