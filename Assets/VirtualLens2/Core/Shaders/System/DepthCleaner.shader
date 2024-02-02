Shader "VirtualLens2/System/DepthCleaner"
{
	Properties
	{
		[Toggle] _IsDesktopMode("Is desktop mode", Float) = 0.0
	}

	SubShader
	{
		Tags
		{
			"RenderType"      = "Opaque"
			"Queue"           = "Geometry"
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
			#include "DepthCleaner.cginc"
			ENDCG
		}

		Pass
		{
			Tags {
				"LightMode" = "ShadowCaster"
			}

			CGPROGRAM
			#pragma vertex   vertex
			#pragma fragment fragment
			#include "DepthCleaner.cginc"
			ENDCG
		}
	}
}
