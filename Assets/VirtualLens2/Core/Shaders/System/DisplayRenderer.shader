Shader "VirtualLens2/System/DisplayRenderer"
{
	Properties
	{
		[NoScaleOffset] _MainTex("Texture", 2D)            = "black" {}
		[NoScaleOffset] _DepthTex("Depth", 2D)             = "black" {}
		[NoScaleOffset] _ComponentTex("Components", 2D)    = "black" {}
		[NoScaleOffset] _CameraPoseTex("CameraPose", 2D)   = "black" {}
		[NoScaleOffset] _StateTex("State", 2D)             = "black" {}
		[NoScaleOffset] _CustomGrid0Tex("CustomGrid0", 2D) = "black" {}
		[NoScaleOffset] _CustomGrid1Tex("CustomGrid1", 2D) = "black" {}
		[NoScaleOffset] _CustomGrid2Tex("CustomGrid2", 2D) = "black" {}
		[NoScaleOffset] _CustomGrid3Tex("CustomGrid3", 2D) = "black" {}
		_GridType         ("Grid Type",          Float) = 0
		_ShowInfo         ("Show Information",   Float) = 0
		_ShowLevel        ("Show Level",         Float) = 0
		_PeakingMode      ("Peaking Mode",       Float) = 0
		_CursorType       ("Cursor Type",        Float) = 0
		_Near             ("Near",               Float) = 0.05
		_Far              ("Far",                Float) = 1000.0
		_FieldOfView      ("Field of View",      Float) = 60.0
		_LogFNumber       ("F Number (log)",     Float) = 2.0
		_LogFocusDistance ("Manual Focus (log)", Float) = 0.0
		_BlurringThresh   ("Max F Number",       Float) = 2.0
		_FocusingThresh   ("Min Focus",          Float) = 0.0
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
			#include "../Common/EmptyPass.cginc"
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
			#include "DisplayRenderer.cginc"
			ENDCG
		}

		Pass
		{
			Tags { "LightMode" = "Vertex" }
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma geometry geometry
			#pragma fragment fragment
			#include "DisplayTextRenderer.cginc"
			ENDCG
		}
	}
}
