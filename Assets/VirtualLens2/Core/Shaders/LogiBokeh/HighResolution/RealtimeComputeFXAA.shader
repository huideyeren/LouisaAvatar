Shader "VirtualLens2/LogiBokeh/HighResolution/RealtimeComputeFXAA"
{
	Properties
	{
		[NoScaleOffset] _DownsampledTex("DownsampledTex", 2D) = "black" {}
		[NoScaleOffset] _CocTex("CocTex", 2D) = "black" {}
		[NoScaleOffset] _TileTex("TileTex", 2D) = "black" {}
		_Blurring("Blur flag", Float) = 0.0
		_Exposure("Exposure Value", Float) = 0.0
		_MaxNumRings("Maximum Number of Rings", int) = 12
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
			Tags { "LightMode" = "ForwardBase" }
			CGPROGRAM
			#pragma vertex   vertex
			#pragma fragment fragment
			#include "../../Common/EmptyPass.cginc"
			ENDCG
		}

		Pass
		{
			Tags { "LightMode" = "Vertex" }
			CGPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#include "RealtimePrefilter.cginc"
			ENDCG
		}

		GrabPass
		{
			Tags { "LightMode" = "Vertex" }
		}

		Pass
		{
			Tags { "LightMode" = "Vertex" }
			CGPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#include "RealtimeBlur.cginc"
			ENDCG
		}

		GrabPass
		{
			Tags { "LightMode" = "Vertex" }
		}

		Pass
		{
			Tags { "LightMode" = "Vertex" }
			CGPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#include "RealtimePostfilter.cginc"
			ENDCG
		}

		GrabPass
		{
			Tags { "LightMode" = "Vertex" }
		}

		Pass
		{
			Tags { "LightMode" = "Vertex" }
			CGPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define USE_FXAA
			#include "RealtimeComposition.cginc"
			ENDCG
		}

		GrabPass
		{
			Tags { "LightMode" = "Vertex" }
		}

		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define REALTIME_FXAA
			#include "FXAA.cginc"
			ENDHLSL
		}
	}
}
