Shader "VirtualLens2/System/CocWriter"
{
	Properties
	{
		[NoScaleOffset] _StateTex("State", 2D) = "black" {}
		_FieldOfView("Field of View", Float) = 60.0
		_LogFNumber("F Number (log)", Float) = 2.0
		_LogFocusDistance("Manual Focus Distance (log)", Float) = 0.0
		_BlurringThresh("Maximum F Number to Blurring", Float) = 0.0
		_FocusingThresh("Minimum Distance to Manual Focusing", Float) = 0.0
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
		Blend  Zero One, One Zero
		Cull   Back
		ZWrite Off
		ZTest  Always

		Pass
		{
			CGPROGRAM
			#pragma vertex   vertex
			#pragma fragment fragment
			#include "CocWriter.cginc"
			ENDCG
		}
	}
}
