Shader "VirtualLens2/System/StateUpdater"
{
	Properties
	{
		[NoScaleOffset] _DepthTex       ("Depth",           2D) = "black" {}
		[NoScaleOffset] _TouchScreenTex ("Touch Screen",    2D) = "black" {}
		[NoScaleOffset] _CameraPoseTex  ("Camera Pose",     2D) = "black" {}
		[NoScaleOffset] _AutoFocusTex   ("AF Buffer",       2D) = "black" {}
		[NoScaleOffset] _SelfieFocusTex ("Selfie Detector", 2D) = "black" {}
		_AFMode           ("Auto Focus Mode",                     Float) = 0
		_AFSpeed          ("Auto Focus Speed",                    Float) = 1.0
		_Near             ("Near",                                Float) = 0.05
		_Far              ("Far",                                 Float) = 1000.0
		_FieldOfView      ("Field of View",                       Float) = 60.0
		_LogFocusDistance ("Manual Focus Distance (log)",         Float) = 0.0
		_FocusingThresh   ("Minimum Distance to Manual Focusing", Float) = 0.0
		_FocusLock        ("Lock Focus Distance",                 Float) = 0
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

		GrabPass
		{
			Tags { "LightMode" = "Vertex" }
		}

		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#pragma shader_feature_local WITH_MULTI_SAMPLING
			#include "StateUpdater.cginc"
			ENDHLSL
		}
	}
}
