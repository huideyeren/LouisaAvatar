Shader "VirtualLens2/System/UnlitPreview"
{
	Properties
	{
		[NoScaleOffset] _MainTex("Texture", 2D) = "black" {}
	}

	SubShader
	{
		Tags
		{
			"RenderType"  = "Transparent"
			"Queue"       = "Overlay+10"
			"VRCFallback" = "Unlit"
		}

		LOD    100

		Pass
		{
			ZTest  Less
			ZWrite On
			Blend  Off

			CGPROGRAM
			#pragma vertex   vertex
			#pragma fragment fragment
			#include "UnlitPreview.cginc"
			ENDCG
		}

		Pass
		{
			ZTest  Always
			ZWrite Off
			Blend  SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex   vertex
			#pragma fragment fragment
			#include "UnlitPreview.cginc"
			ENDCG
		}
	}
}
