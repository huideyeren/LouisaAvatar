Shader "NN4VRC/YOLOX_Nano_Visualizer"
{
	Properties
	{
		[NoScaleOffset] _InputTex   ("input",      2D) = "black" {}
		[NoScaleOffset] _Prediction ("prediction", 2D) = "black" {}
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

		Pass {
			Tags { "LightMode" = "ForwardBase" }
			CGPROGRAM
			#pragma vertex   vertex
			#pragma fragment fragment
			#include "../../Common/EmptyPass.cginc"
			ENDCG
		}

		Pass {
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma geometry geometry
			#pragma fragment fragment
			#define INPUT_WIDTH   (2048)
			#define INPUT_HEIGHT  (1152)
			#define OUTPUT_WIDTH  (1920)
			#define OUTPUT_HEIGHT (1080)
			#define INPUT_TEXTURE _InputTex
			#include "VisualizeSource.cginc"
			ENDHLSL
		}

		Pass {
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma geometry geometry
			#pragma fragment fragment
			#define INPUT_WIDTH        (480)
			#define INPUT_HEIGHT       (256)
			#define OUTPUT_WIDTH       (1920)
			#define OUTPUT_HEIGHT      (1080)
			#define INV_SCALE          (4)
			#define PREDICTION_ROWS    (0x00a8)
			#define PREDICTION_TEXTURE _Prediction
			#include "Config.cginc"
			#include "VisualizeBox.cginc"
			ENDHLSL
		}
	}
}
