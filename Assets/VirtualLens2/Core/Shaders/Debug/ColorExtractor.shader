Shader "VirtualLens2/Debug/ColorExtractor"
{
	Properties
	{
		[NoScaleOffset] _MainTex("Texture", 2D) = "black" {}
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
			#include "ColorExtractor.cginc"
			ENDCG
		}
	}
}
