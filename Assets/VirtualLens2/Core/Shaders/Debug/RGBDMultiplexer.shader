Shader "VirtualLens2/Debug/RGBDMultiplexer"
{
	Properties
	{
		[NoScaleOffset] _MainTex("Texture", 2D) = "black" {}
		[NoScaleOffset] _DepthTex("Texture", 2D) = "black" {}
		_Near("Near", Float) = 0.05
		_Far("Far", Float) = 1000.0
	}

	SubShader
	{
		Cull   Off
		ZWrite On
		ZTest  Always

		Pass
		{
			CGPROGRAM
			#pragma vertex   vertex
			#pragma fragment fragment
			#include "RGBDMultiplexer.cginc"
			ENDCG
		}
	}
}
