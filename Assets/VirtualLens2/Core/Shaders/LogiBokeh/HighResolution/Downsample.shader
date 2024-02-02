Shader "VirtualLens2/LogiBokeh/HighResolution/Downsample"
{
	Properties
	{
		[NoScaleOffset] _MainTex("MainTex", 2D) = "black" {}
		[NoScaleOffset] _CocTex("CocTex", 2D) = "black" {}
		[NoScaleOffset] _DepthTex("DepthTex", 2D) = "black" {}
		[NoScaleOffset] _TileTex("TileTex", 2D) = "black" {}
		[NoScaleOffset] _StateTex("StateTex", 2D) = "black" {}
		_Near("Near", Float) = 0.05
		_Far("Far", Float) = 1000.0
		_FieldOfView("Field of View", Float) = 60.0
		_LogFNumber("F Number (log)", Float) = 2.0
		_LogFocusDistance("Manual Focus Distance (log)", Float) = 2.0
		_BlurringThresh("Maximum F Number for Blurring", Float) = 0.0
		_FocusingThresh("Minimum Distance for Manual Focusing", Float) = 0.0
		_Blurring("Blur flag", Float) = 0.0
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
			#pragma shader_feature_local WITH_MULTI_SAMPLING
			#include "Downsample.cginc"
			ENDCG
		}

		Pass
		{
			Tags { "LightMode" = "Vertex" }
			CGPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#pragma shader_feature_local WITH_MULTI_SAMPLING
			#include "Prefilter.cginc"
			ENDCG
		}
	}
}
