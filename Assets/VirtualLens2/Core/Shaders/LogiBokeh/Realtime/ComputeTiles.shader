Shader "VirtualLens2/LogiBokeh/Realtime/ComputeTiles"
{
	Properties
	{
		[NoScaleOffset] _CocTex("CocTex", 2D) = "black" {}
		_Blurring("Blur flag", Float) = 0.0
		_MaxNumRings("Maximum Number of Rings", int) = 6
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
			#include "AggregateTiles.cginc"
			ENDCG
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		Pass
		{
			Tags { "LightMode" = "Vertex" }
			CGPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#include "DilateTiles.cginc"
			ENDCG
		}
	}
}
