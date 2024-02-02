#ifndef NN4VRC_COMMON_ACTIVATIONS_SILU_CGINC
#define NN4VRC_COMMON_ACTIVATIONS_SILU_CGINC

float activation(float x){
	return x / (1.0 + exp(-x));
}

float2 activation(float2 x){
	return x / (1.0 + exp(-x));
}

float3 activation(float3 x){
	return x / (1.0 + exp(-x));
}

float4 activation(float4 x){
	return x / (1.0 + exp(-x));
}

#endif
