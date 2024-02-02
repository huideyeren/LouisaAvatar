#ifndef NN4VRC_COMMON_HALF_CGINC
#define NN4VRC_COMMON_HALF_CGINC

//---------------------------------------------------------------------------
// Half2 packing
//---------------------------------------------------------------------------

float2 unpack_half2(float x){
	const uint u = asuint(x);
	return float2(f16tof32(u & 0xffff), f16tof32(u >> 16));
}

float pack_half2(float2 x){
	// Round-to-nearest conversion should be used but f32tof16 uses round-to-zero.
	const float x0 = asfloat(asuint(x.x) + (1u << 12));
	const float x1 = asfloat(asuint(x.y) + (1u << 12));
	return asfloat(f32tof16(x0) | (f32tof16(x1) << 16));
}

#endif
