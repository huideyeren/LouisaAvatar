Shader "VirtualLens2/System/CameraPoseReceiver"
{
	Properties
	{
		_OffsetY("Offset Y", Float) = 0
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
			#include "CameraPoseReceiver.cginc"
			ENDCG
		}
	}
}
