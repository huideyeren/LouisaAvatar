#ifndef VIRTUALLENS2_COMMON_STATE_TEXTURE_CGINC
#define VIRTUALLENS2_COMMON_STATE_TEXTURE_CGINC

#include "UnityCG.cginc"

//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

Texture2D<float4> _StateTex;


//---------------------------------------------------------------------------
// State texture mapping:
//
// (0, 0): Touching point in screen space
//   - X: Initialized?
//   - Y: Focus point (u)
//   - Z: Focus point (v)
//
// (1, 0): Focus point in world space
//   - XYZ: Focus point
//   - W: Timestamp
//
// (2, 0): Bounding box from AF engine
//   - XY: Center position (in uv space)
//   - ZW: Width and height (in uv space)
//
// (3, 0): Focus distance and timestamps
//   - X: Current focus distance [m]
//   - Y: Timestamp for last face detection
//   - Z: Timestamp for last pixel detection
//
//---------------------------------------------------------------------------

// Focus point
float2 getFocusPointState(){
	const float4 raw = _StateTex[int2(0, 0)];
	if(raw.x < 0.5){
		return float2(0.5, 0.5);
	}else{
		return raw.yz;
	}
}

float3 getFocusPointInWorld(){
	return _StateTex[int2(1, 0)].xyz;
}

float getLastFaceTrackedTimestamp(){
	return _StateTex[int2(3, 0)].y;
}

float getLastPixelTrackedTimestamp(){
	return _StateTex[int2(3, 0)].z;
}

float4 getDetectionBoundary(){
	return _StateTex[int2(2, 0)];
}

float getFocusDistance(){
	return _StateTex[int2(3, 0)].x;
}

#endif
