Shader "VirtualLens2/Debug/Depth2Coc"
{
	Properties
	{
		[NoScaleOffset] _MainTex("Texture", 2D) = "black" {}
		_FieldOfView("Field of View", Float) = 60.0
		_FNumber("F Number", Float) = 2.0
		_FocusDistance("Manual Focus Distance", Float) = 0.0
	}

	SubShader
	{
		Cull   Off
		ZWrite Off
		ZTest  Always

		Pass
		{
			CGPROGRAM
			#pragma vertex   CustomRenderTextureVertexShader
			#pragma fragment fragment
			#include "Depth2Coc.cginc"
			ENDCG
		}
	}
}
