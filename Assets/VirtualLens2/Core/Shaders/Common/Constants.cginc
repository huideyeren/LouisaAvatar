#ifndef VIRTUALLENS2_COMMON_CONSTANTS_CGINC
#define VIRTUALLENS2_COMMON_CONSTANTS_CGINC

static const float PI      = 3.14159265359;
static const float DEG2RAD = 0.01745329251;

static const float FLT_MAX = 3.402823466e+38;

static const float SENSOR_SIZE = 0.036;  // Width of the image sensor [m]

static const float ASPECT     = 16.0 / 9.0;
static const float RCP_ASPECT = 1.0 / ASPECT;

static const float2 SCREEN_ENLARGEMENT     = float2(2048.0 / 1920.0, 1152.0 / 1080.0);
static const float2 INV_SCREEN_ENLARGEMENT = 1.0 / SCREEN_ENLARGEMENT;

// Maximum CoC radiuses in half resolution
static const float MAX_FOREGROUND_RADIUS = 15.0f;
static const float MAX_BACKGROUND_RADIUS = 40.0f;

#endif
