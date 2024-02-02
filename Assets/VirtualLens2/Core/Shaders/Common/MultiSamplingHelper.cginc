#ifndef VIRTUALLENS2_COMMON_MULTI_SAMPLING_HELPER_CGINC
#define VIRTUALLENS2_COMMON_MULTI_SAMPLING_HELPER_CGINC

#include "UnityCG.cginc"

//---------------------------------------------------------------------------
// Preprocessor definitions
//---------------------------------------------------------------------------

#ifdef WITH_MULTI_SAMPLING

#define TEXTURE2DMS Texture2DMS
#define TEXTURE2DMS_COORD(coord) (coord)
#define TEXTURE2DMS_GET_DIMENSIONS(tex, width, height, num_samples) \
	((tex).GetDimensions(width, height, num_samples))

#else

#define TEXTURE2DMS Texture2D
#define TEXTURE2DMS_COORD(coord) (int3(coord, 0))
#define TEXTURE2DMS_GET_DIMENSIONS(tex, width, height, num_samples) \
	do { \
		(tex).GetDimensions(width, height); \
		num_samples = 1; \
	} while(false)

#endif

#endif
