Shader "VirtualLens2/Debug/DepthExtractor"
{
	Properties
	{
		[NoScaleOffset] _MainTex("Texture", 2D) = "black" {}
		_Near("Near [m]", Float) = 0.05
		_Far("Far [m]", Float) = 1000.0
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
			#include "DepthExtractor.cginc"
			ENDCG
		}
	}
}
