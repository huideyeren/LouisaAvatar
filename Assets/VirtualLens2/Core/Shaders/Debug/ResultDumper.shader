Shader "VirtualLens2/Debug/ResultDumper"
{
	Properties
	{
		[NoScaleOffset] _MainTex("Texture", 2D) = "black" {}
	}

	SubShader
	{
		Tags
		{
			"RenderType"      = "Transparent"
			"Queue"           = "Overlay+1000"
			"DisableBatching" = "True"
			"IgnoreProjector" = "True"
		}

		LOD    100
		Blend  Off
		Cull   Back
		ZWrite On
		ZTest  Always

		Pass
		{
			CGPROGRAM
			#pragma vertex   vertex
			#pragma fragment fragment
			#include "ResultDumper.cginc"
			ENDCG
		}
	}
}
