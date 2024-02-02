Shader "VirtualLens2/LogiBokeh/HighResolution/HighResolutionCompute"
{
	Properties
	{
		[NoScaleOffset] _MainTex("MainTex", 2D) = "black" {}
		[NoScaleOffset] _DownsampledTex("DownsampledTex", 2D) = "black" {}
		[NoScaleOffset] _CocTex("CocTex", 2D) = "black" {}
		[NoScaleOffset] _TileTex("TileTex", 2D) = "black" {}
		_Blurring("Blur flag", Float) = 0.0
		_Exposure("Exposure Value", Float) = 0.0
		_MaxNumRings("Maximum Number of Rings", int) = 12
		[Toggle] _IsDesktopMode("Is desktop mode", Float) = 0.0
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
		ZWrite Off
		ZTest  Always

		Pass
		{
			Tags { "LightMode" = "Always" }
			CGPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#include "Blur.cginc"
			ENDCG
		}

		GrabPass { "_VirtualLens2_WorkareaTex" }

		Pass
		{
			Tags { "LightMode" = "Always" }
			ZWrite On
			CGPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#pragma shader_feature_local WITH_MULTI_SAMPLING
			#define FUSED_POSTFILTERING
			#include "Composition.cginc"
			ENDCG
		}
	}
}
