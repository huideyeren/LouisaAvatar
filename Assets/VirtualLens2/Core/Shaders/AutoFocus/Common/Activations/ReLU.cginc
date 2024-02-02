#ifndef NN4VRC_COMMON_ACTIVATIONS_RELU_CGINC
#define NN4VRC_COMMON_ACTIVATIONS_RELU_CGINC

float activation(float x){
	return max(x, 0);
}

float2 activation(float2 x){
	return max(x, 0);
}

float3 activation(float3 x){
	return max(x, 0);
}

float4 activation(float4 x){
	return max(x, 0);
}

#endif
