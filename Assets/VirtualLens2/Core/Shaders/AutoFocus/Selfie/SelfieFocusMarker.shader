Shader "VirtualLens2/AutoFocus/SelfieFocusMarker"
{
	Properties
	{
		_EyeIndex("Eye Index", Int) = 0
	}

	SubShader
	{
		Tags
		{
			"RenderType"      = "Transparent"
			"Queue"           = "Overlay+1000"
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
			#include "SelfieFocusMarker.cginc"
			ENDCG
		}
	}
}
