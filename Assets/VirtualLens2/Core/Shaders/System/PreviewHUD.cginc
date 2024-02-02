#ifndef VIRTUALLENS2_SYSTEM_PREVIEW_HUD_CGINC
#define VIRTUALLENS2_SYSTEM_PREVIEW_HUD_CGINC

#include "UnityCG.cginc"
#include "../Common/Constants.cginc"
#include "../Common/TargetDetector.cginc"


//---------------------------------------------------------------------------
// Parameter declarations
//---------------------------------------------------------------------------

float3 _Scale;
float _Roll;

Texture2D _MainTex;

SamplerState linear_clamp_sampler;


//---------------------------------------------------------------------------
// Structure definitions
//---------------------------------------------------------------------------

struct appdata {
	float4 vertex : POSITION;
	float2 uv     : TEXCOORD0;
};

struct v2f {
	float4 vertex : SV_POSITION;
	float2 uv     : TEXCOORD0;
};


//---------------------------------------------------------------------------
// Vertex shader
//---------------------------------------------------------------------------

#if UNITY_SINGLE_PASS_STEREO

float4x4 computeFaceToWorld(){
	const float4x4 a = unity_StereoCameraToWorld[0];
	const float4x4 b = unity_StereoCameraToWorld[1];

	float3 iy = (a._m01_m11_m21 + b._m01_m11_m21) * 0.5;
	float3 iz = (a._m02_m12_m22 + b._m02_m12_m22) * 0.5;
	float3 ix = normalize(cross(iy, iz));
	iz = normalize(iz);
	iy = cross(iz, ix);

	float3 pos = (a._m03_m13_m23 + b._m03_m13_m23) * 0.5;

	return float4x4(
		float4(ix.x, iy.x, iz.x, pos.x),
		float4(ix.y, iy.y, iz.y, pos.y),
		float4(ix.z, iy.z, iz.z, pos.z),
		float4(0.0, 0.0, 0.0, 1.0));
}

float computeFaceScale(){
	const float4x4 a = unity_StereoCameraToWorld[0];
	const float4x4 b = unity_StereoCameraToWorld[1];
	const float3 d = a._m03_m13_m23 - b._m03_m13_m23;
	return length(d);
}

#else

float4x4 computeFaceToWorld(){
	return unity_CameraToWorld;
}

float computeFaceScale(){
	return 0.063;
}

#endif

v2f vertex(appdata v){
	v2f o;

	if(!testStereo() || testInMirror()) {
		o.vertex = 0;
		o.uv     = 0;
		return o;
	}

	const float3 scale = _Scale * computeFaceScale();

	const int roll = (int)round(_Roll) & 3;
	float2 pos = float2(
		scale.x * (v.uv.x * 2 - 1) * ASPECT,
		scale.y * (v.uv.y * 2 - 1) * _ProjectionParams.x);
	if(roll == 0){
	}else if(roll == 1){
		pos = float2(-pos.y, pos.x);
	}else if(roll == 2){
		pos = float2(-pos.x, -pos.y);
	}else if(roll == 3){
		pos = float2(pos.y, -pos.x);
	}

	const float4x4 f2w = computeFaceToWorld();

	const float3 front_f = float3(0.0, 0.0, 1.0);
	const float3 front_w = mul(f2w, float4(front_f, 0.0)).xyz;
	const float3 right_w = normalize(cross(float3(0.0, 1.0, 0.0), front_w));
	const float3 up_w    = normalize(cross(right_w, front_w));

	const float3 center_f = float3(0.0, 0.0, scale.z);
	const float3 center_w = mul(f2w, float4(center_f, 1.0)).xyz;
	const float3 pos_w = center_w + right_w * pos.x + up_w * pos.y;
	const float4 pos_v = mul(unity_MatrixVP, float4(pos_w, 1.0));

	o.vertex = pos_v;
	o.uv     = v.uv;
	return o;
}


//---------------------------------------------------------------------------
// Fragment shader
//---------------------------------------------------------------------------

float4 fragment(v2f i) : SV_Target {
	return _MainTex.Sample(linear_clamp_sampler, i.uv);
}

#endif
