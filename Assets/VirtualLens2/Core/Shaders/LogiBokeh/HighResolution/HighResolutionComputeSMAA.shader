Shader "VirtualLens2/LogiBokeh/HighResolution/HighResolutionComputeSMAA"
{
	Properties
	{
		[NoScaleOffset] _MainTex("MainTex", 2D) = "black" {}
		[NoScaleOffset] _DownsampledTex("DownsampledTex", 2D) = "black" {}
		[NoScaleOffset] _CocTex("CocTex", 2D) = "black" {}
		[NoScaleOffset] _TileTex("TileTex", 2D) = "black" {}
		[NoScaleOffset] _SMAAAreaTex("SMAAAreaTex", 2D) = "black" {}
		[NoScaleOffset] _SMAASearchTex("SMAASearchTex", 2D) = "black" {}
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
			CGPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#pragma shader_feature_local WITH_MULTI_SAMPLING
			#define FUSED_POSTFILTERING
			#define USE_SMAA
			#include "Composition.cginc"
			ENDCG
		}

		GrabPass { "_VirtualLens2_BlurredTex" }

		Pass
		{
			Tags { "LightMode" = "Always" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define HIGH_RESOLUTION_SMAA
			#include "SMAA.cginc"
			#include "SMAAEdgeDetection.cginc"
			ENDHLSL
		}

		GrabPass { "_VirtualLens2_SMAAEdgeTex" }

		Pass
		{
			Tags { "LightMode" = "Always" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define HIGH_RESOLUTION_SMAA
			#include "SMAA.cginc"
			#include "SMAABlendingWeightCalculation.cginc"
			ENDHLSL
		}

		GrabPass { "_VirtualLens2_SMAAWeightTex" }

		Pass
		{
			Tags { "LightMode" = "Always" }
			ZWrite On
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define HIGH_RESOLUTION_SMAA
			#include "SMAA.cginc"
			#include "SMAANeighborhoodBlending.cginc"
			ENDHLSL
		}
	}
}
