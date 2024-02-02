Shader "VirtualLens2/System/PreviewHUD"
{
	Properties
	{
		[NoScaleOffset] _MainTex("Texture", 2D) = "black" {}
		_Scale("Scale", Vector) = (1.0, 1.0, 1.0, 1.0)
		_Roll("Roll", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"RenderType"      = "Transparent"
			"Queue"           = "Overlay+5"
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
			#include "PreviewHUD.cginc"
			ENDCG
		}
	}
}
