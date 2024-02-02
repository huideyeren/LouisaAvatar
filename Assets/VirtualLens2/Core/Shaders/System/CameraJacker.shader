Shader "VirtualLens2/System/CameraJacker"
{
	Properties
	{
		[NoScaleOffset] _MainTex("Texture", 2D) = "black" {}
		[Toggle] _Ignore4KScreen("Ignore 4K screen", Float) = 0.0
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
		ZWrite On
		ZTest  Always

		Pass
		{
			CGPROGRAM
			#pragma vertex   vertex
			#pragma fragment fragment
			#include "CameraJacker.cginc"
			ENDCG
		}
	}
}
