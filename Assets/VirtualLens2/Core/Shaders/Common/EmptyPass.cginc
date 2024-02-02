#ifndef VIRTUALLENS2_COMMON_EMPTY_PASS_CGINC
#define VIRTUALLENS2_COMMON_EMPTY_PASS_CGINC

float4 vertex() : SV_POSITION { return 0; }
float4 fragment() : SV_TARGET { return 0; }

#endif
