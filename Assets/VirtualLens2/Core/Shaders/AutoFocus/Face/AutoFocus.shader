Shader "VirtualLens2/AutoFocus/FaceDetector"
{
	Properties
	{
		[NoScaleOffset] _InputTex       ("input",     2D) = "black" {}
		[NoScaleOffset] _DepthTex       ("depth",     2D) = "black" {}
		[NoScaleOffset] _StateTex       ("state",     2D) = "black" {}
		[NoScaleOffset] _TouchScreenTex ("touch",     2D) = "black" {}
		[NoScaleOffset] _CameraPoseTex  ("pose",      2D) = "black" {}
		[NoScaleOffset] _Weights        ("weights",   2D) = "black" {}
		[NoScaleOffset] _Biases         ("biases",    2D) = "black" {}
		[NoScaleOffset] _Predictor      ("predictor", 2D) = "black" {}
		[Toggle] _Use_Linear_Input ("Use Linear Input", Int) = 0
		[Toggle] _Use_4K_Input     ("Use 4K Input",     Int) = 0
		_Near           ("Near",             Float) = 0.05
		_Far            ("Far",              Float) = 1000.0
		_FieldOfView    ("Field of View",    Float) = 60.0
		_TrackingSpeed  ("Tracking Speed",   Float) = 5.0
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

		// force to ignore passes with "LightMode"="Vertex" for non-compute cameras
		Pass
		{
			Tags { "LightMode" = "ForwardBase" }
			CGPROGRAM
			#pragma vertex   vertex
			#pragma fragment fragment
			#include "../../Common/EmptyPass.cginc"
			ENDCG
		}

		// preprocess
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#pragma shader_feature_local WITH_MULTI_SAMPLING
			#define DESTINATION_OFFSET (0x0000)
			#define OUTPUT_WIDTH  (480)
			#define OUTPUT_HEIGHT (256)
			#define INPUT_TEXTURE _InputTex
			#include "Config.cginc"
			#include "Preprocess.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.stem.rearrange
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x0000)
			#define DESTINATION_OFFSET (0x0000)
			#define INPUT_WIDTH        (480)
			#define INPUT_HEIGHT       (256)
			#define INPUT_TEXTURE      _GrabTexture
			#include "Config.cginc"
			#include "Rearrange.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.stem.conv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x0000)
			#define DESTINATION_OFFSET (0x1000)
			#define INPUT_WIDTH        (242)
			#define INPUT_HEIGHT       (130)
			#define INPUT_CHANNELS     (16)
			#define OUTPUT_CHANNELS    (16)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (16)
			#define WEIGHT_OFFSET_Y    (864)
			#define BIAS_OFFSET_X      (112)
			#define BIAS_OFFSET_Y      (30)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution3x3.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark2.0.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1000)
			#define DESTINATION_OFFSET (0x1c00)
			#define INPUT_WIDTH        (240)
			#define INPUT_HEIGHT       (128)
			#define INPUT_CHANNELS     (16)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (48)
			#define WEIGHT_OFFSET_Y    (824)
			#define BIAS_OFFSET_X      (8)
			#define BIAS_OFFSET_Y      (29)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3s2p1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark2.0.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1c00)
			#define DESTINATION_OFFSET (0x1800)
			#define INPUT_WIDTH        (120)
			#define INPUT_HEIGHT       (64)
			#define INPUT_CHANNELS     (16)
			#define OUTPUT_CHANNELS    (32)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (124)
			#define WEIGHT_OFFSET_Y    (768)
			#define BIAS_OFFSET_X      (96)
			#define BIAS_OFFSET_Y      (29)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark2.1.conv1
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1800)
			#define DESTINATION_OFFSET (0x1800)
			#define INPUT_WIDTH        (120)
			#define INPUT_HEIGHT       (64)
			#define INPUT_CHANNELS     (32)
			#define OUTPUT_CHANNELS    (16)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (32)
			#define WEIGHT_OFFSET_Y    (848)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (29)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		// backbone.backbone.dark2.1.conv2
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1800)
			#define DESTINATION_OFFSET (0x1c00)
			#define INPUT_WIDTH        (120)
			#define INPUT_HEIGHT       (64)
			#define INPUT_CHANNELS     (32)
			#define OUTPUT_CHANNELS    (16)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (52)
			#define WEIGHT_OFFSET_Y    (848)
			#define BIAS_OFFSET_X      (40)
			#define BIAS_OFFSET_Y      (29)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark2.1.m.0.conv1
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1800)
			#define DESTINATION_OFFSET (0x1400)
			#define INPUT_WIDTH        (120)
			#define INPUT_HEIGHT       (64)
			#define INPUT_CHANNELS     (16)
			#define OUTPUT_CHANNELS    (16)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (124)
			#define WEIGHT_OFFSET_Y    (824)
			#define BIAS_OFFSET_X      (32)
			#define BIAS_OFFSET_Y      (29)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark2.1.m.0.conv2.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1400)
			#define DESTINATION_OFFSET (0x1400)
			#define INPUT_WIDTH        (120)
			#define INPUT_HEIGHT       (64)
			#define INPUT_CHANNELS     (16)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (48)
			#define WEIGHT_OFFSET_Y    (816)
			#define BIAS_OFFSET_X      (72)
			#define BIAS_OFFSET_Y      (29)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3p1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark2.1.m.0.conv2.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1400)
			#define DESTINATION_OFFSET (0x1800)
			#define INPUT_WIDTH        (120)
			#define INPUT_HEIGHT       (64)
			#define INPUT_CHANNELS     (16)
			#define OUTPUT_CHANNELS    (16)
			#define FUSED_ADD
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (124)
			#define WEIGHT_OFFSET_Y    (816)
			#define BIAS_OFFSET_X      (64)
			#define BIAS_OFFSET_Y      (29)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark2.1.conv3
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1800)
			#define DESTINATION_OFFSET (0x1800)
			#define INPUT_WIDTH        (120)
			#define INPUT_HEIGHT       (64)
			#define INPUT_CHANNELS     (32)
			#define OUTPUT_CHANNELS    (32)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (116)
			#define WEIGHT_OFFSET_Y    (848)
			#define BIAS_OFFSET_X      (80)
			#define BIAS_OFFSET_Y      (29)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark3.0.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1800)
			#define DESTINATION_OFFSET (0x1e00)
			#define INPUT_WIDTH        (120)
			#define INPUT_HEIGHT       (64)
			#define INPUT_CHANNELS     (32)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (48)
			#define WEIGHT_OFFSET_Y    (800)
			#define BIAS_OFFSET_X      (48)
			#define BIAS_OFFSET_Y      (29)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3s2p1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark3.0.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1e00)
			#define DESTINATION_OFFSET (0x1c00)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (32)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (51)
			#define WEIGHT_OFFSET_Y    (640)
			#define BIAS_OFFSET_X      (96)
			#define BIAS_OFFSET_Y      (27)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark3.1.conv1
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1c00)
			#define DESTINATION_OFFSET (0x1c00)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (32)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (32)
			#define WEIGHT_OFFSET_Y    (832)
			#define BIAS_OFFSET_X      (16)
			#define BIAS_OFFSET_Y      (29)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		// backbone.backbone.dark3.1.conv2
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1c00)
			#define DESTINATION_OFFSET (0x1e00)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (32)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (108)
			#define WEIGHT_OFFSET_Y    (832)
			#define BIAS_OFFSET_X      (112)
			#define BIAS_OFFSET_Y      (29)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark3.1.m.0.conv1
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1c00)
			#define DESTINATION_OFFSET (0x1a00)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (32)
			#define OUTPUT_CHANNELS    (32)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (108)
			#define WEIGHT_OFFSET_Y    (848)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (28)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark3.1.m.0.conv2.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1a00)
			#define DESTINATION_OFFSET (0x1a00)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (32)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (124)
			#define WEIGHT_OFFSET_Y    (800)
			#define BIAS_OFFSET_X      (32)
			#define BIAS_OFFSET_Y      (28)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3p1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark3.1.m.0.conv2.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1a00)
			#define DESTINATION_OFFSET (0x1c00)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (32)
			#define OUTPUT_CHANNELS    (32)
			#define FUSED_ADD
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (8)
			#define WEIGHT_OFFSET_Y    (832)
			#define BIAS_OFFSET_X      (64)
			#define BIAS_OFFSET_Y      (28)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark3.1.m.1.conv1
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1c00)
			#define DESTINATION_OFFSET (0x1a00)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (32)
			#define OUTPUT_CHANNELS    (32)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (0)
			#define WEIGHT_OFFSET_Y    (832)
			#define BIAS_OFFSET_X      (112)
			#define BIAS_OFFSET_Y      (28)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark3.1.m.1.conv2.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1a00)
			#define DESTINATION_OFFSET (0x1a00)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (32)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (48)
			#define WEIGHT_OFFSET_Y    (784)
			#define BIAS_OFFSET_X      (96)
			#define BIAS_OFFSET_Y      (28)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3p1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark3.1.m.1.conv2.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1a00)
			#define DESTINATION_OFFSET (0x1c00)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (32)
			#define OUTPUT_CHANNELS    (32)
			#define FUSED_ADD
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (51)
			#define WEIGHT_OFFSET_Y    (832)
			#define BIAS_OFFSET_X      (80)
			#define BIAS_OFFSET_Y      (28)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark3.1.m.2.conv1
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1c00)
			#define DESTINATION_OFFSET (0x1a00)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (32)
			#define OUTPUT_CHANNELS    (32)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (51)
			#define WEIGHT_OFFSET_Y    (816)
			#define BIAS_OFFSET_X      (48)
			#define BIAS_OFFSET_Y      (28)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark3.1.m.2.conv2.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1a00)
			#define DESTINATION_OFFSET (0x1a00)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (32)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (124)
			#define WEIGHT_OFFSET_Y    (784)
			#define BIAS_OFFSET_X      (16)
			#define BIAS_OFFSET_Y      (28)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3p1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark3.1.m.2.conv2.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1a00)
			#define DESTINATION_OFFSET (0x1c00)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (32)
			#define OUTPUT_CHANNELS    (32)
			#define FUSED_ADD
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (51)
			#define WEIGHT_OFFSET_Y    (800)
			#define BIAS_OFFSET_X      (16)
			#define BIAS_OFFSET_Y      (27)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark3.1.conv3
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1c00)
			#define DESTINATION_OFFSET (0x1c00)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (76)
			#define WEIGHT_OFFSET_Y    (832)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (26)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark4.0.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1c00)
			#define DESTINATION_OFFSET (0x1b00)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (48)
			#define WEIGHT_OFFSET_Y    (736)
			#define BIAS_OFFSET_X      (32)
			#define BIAS_OFFSET_Y      (26)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3s2p1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark4.0.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1b00)
			#define DESTINATION_OFFSET (0x1a00)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (128)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (32)
			#define WEIGHT_OFFSET_Y    (576)
			#define BIAS_OFFSET_X      (64)
			#define BIAS_OFFSET_Y      (13)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark4.1.conv1
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1a00)
			#define DESTINATION_OFFSET (0x1a00)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (128)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (60)
			#define WEIGHT_OFFSET_Y    (736)
			#define BIAS_OFFSET_X      (64)
			#define BIAS_OFFSET_Y      (26)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		// backbone.backbone.dark4.1.conv2
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1a00)
			#define DESTINATION_OFFSET (0x1b00)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (128)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (0)
			#define WEIGHT_OFFSET_Y    (704)
			#define BIAS_OFFSET_X      (96)
			#define BIAS_OFFSET_Y      (26)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark4.1.m.0.conv1
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1a00)
			#define DESTINATION_OFFSET (0x1900)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (92)
			#define WEIGHT_OFFSET_Y    (832)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (25)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark4.1.m.0.conv2.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1900)
			#define DESTINATION_OFFSET (0x1900)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (124)
			#define WEIGHT_OFFSET_Y    (736)
			#define BIAS_OFFSET_X      (32)
			#define BIAS_OFFSET_Y      (25)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3p1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark4.1.m.0.conv2.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1900)
			#define DESTINATION_OFFSET (0x1a00)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (64)
			#define FUSED_ADD
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (16)
			#define WEIGHT_OFFSET_Y    (832)
			#define BIAS_OFFSET_X      (64)
			#define BIAS_OFFSET_Y      (25)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark4.1.m.1.conv1
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1a00)
			#define DESTINATION_OFFSET (0x1900)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (60)
			#define WEIGHT_OFFSET_Y    (832)
			#define BIAS_OFFSET_X      (96)
			#define BIAS_OFFSET_Y      (25)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark4.1.m.1.conv2.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1900)
			#define DESTINATION_OFFSET (0x1900)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (51)
			#define WEIGHT_OFFSET_Y    (736)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (24)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3p1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark4.1.m.1.conv2.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1900)
			#define DESTINATION_OFFSET (0x1a00)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (64)
			#define FUSED_ADD
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (0)
			#define WEIGHT_OFFSET_Y    (800)
			#define BIAS_OFFSET_X      (32)
			#define BIAS_OFFSET_Y      (24)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark4.1.m.2.conv1
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1a00)
			#define DESTINATION_OFFSET (0x1900)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (32)
			#define WEIGHT_OFFSET_Y    (800)
			#define BIAS_OFFSET_X      (64)
			#define BIAS_OFFSET_Y      (24)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark4.1.m.2.conv2.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1900)
			#define DESTINATION_OFFSET (0x1900)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (54)
			#define WEIGHT_OFFSET_Y    (736)
			#define BIAS_OFFSET_X      (96)
			#define BIAS_OFFSET_Y      (24)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3p1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark4.1.m.2.conv2.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1900)
			#define DESTINATION_OFFSET (0x1a00)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (64)
			#define FUSED_ADD
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (108)
			#define WEIGHT_OFFSET_Y    (800)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (23)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark4.1.conv3
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1a00)
			#define DESTINATION_OFFSET (0x1a00)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (128)
			#define OUTPUT_CHANNELS    (128)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (64)
			#define WEIGHT_OFFSET_Y    (576)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (12)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark5.0.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1a00)
			#define DESTINATION_OFFSET (0x1980)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (128)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (57)
			#define WEIGHT_OFFSET_Y    (576)
			#define BIAS_OFFSET_X      (64)
			#define BIAS_OFFSET_Y      (12)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3s2p1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark5.0.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1980)
			#define DESTINATION_OFFSET (0x1900)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (128)
			#define OUTPUT_CHANNELS    (256)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (0)
			#define WEIGHT_OFFSET_Y    (256)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (3)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark5.1.conv1
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1900)
			#define DESTINATION_OFFSET (0x1800)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (256)
			#define OUTPUT_CHANNELS    (128)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (64)
			#define WEIGHT_OFFSET_Y    (192)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (11)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark5.1.m.0
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1800)
			#define DESTINATION_OFFSET (0x1880)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (128)
			#define INPUT_TEXTURE      _GrabTexture
			#include "Config.cginc"
			#include "../Common/MaxPool5x5p2.cginc"
			ENDHLSL
		}

		// backbone.backbone.dark5.1.m.1
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1800)
			#define DESTINATION_OFFSET (0x1900)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (128)
			#define INPUT_TEXTURE      _GrabTexture
			#include "Config.cginc"
			#include "../Common/MaxPool9x9p4.cginc"
			ENDHLSL
		}

		// backbone.backbone.dark5.1.m.2
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1800)
			#define DESTINATION_OFFSET (0x1980)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (128)
			#define INPUT_TEXTURE      _GrabTexture
			#include "Config.cginc"
			#include "../Common/MaxPool13x13p6.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark5.1.conv2
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1800)
			#define DESTINATION_OFFSET (0x1900)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (512)
			#define OUTPUT_CHANNELS    (256)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (0)
			#define WEIGHT_OFFSET_Y    (0)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (2)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark5.2.conv1
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1900)
			#define DESTINATION_OFFSET (0x1900)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (256)
			#define OUTPUT_CHANNELS    (128)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (0)
			#define WEIGHT_OFFSET_Y    (448)
			#define BIAS_OFFSET_X      (64)
			#define BIAS_OFFSET_Y      (11)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		// backbone.backbone.dark5.2.conv2
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1900)
			#define DESTINATION_OFFSET (0x1980)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (256)
			#define OUTPUT_CHANNELS    (128)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (64)
			#define WEIGHT_OFFSET_Y    (448)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (10)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark5.2.m.0.conv1
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1900)
			#define DESTINATION_OFFSET (0x1900)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (128)
			#define OUTPUT_CHANNELS    (128)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (96)
			#define WEIGHT_OFFSET_Y    (576)
			#define BIAS_OFFSET_X      (64)
			#define BIAS_OFFSET_Y      (10)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark5.2.m.0.conv2.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1900)
			#define DESTINATION_OFFSET (0x1900)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (128)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (54)
			#define WEIGHT_OFFSET_Y    (576)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (9)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3p1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark5.2.m.0.conv2.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1900)
			#define DESTINATION_OFFSET (0x1900)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (128)
			#define OUTPUT_CHANNELS    (128)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (32)
			#define WEIGHT_OFFSET_Y    (320)
			#define BIAS_OFFSET_X      (64)
			#define BIAS_OFFSET_Y      (9)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.backbone.dark5.2.conv3
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1900)
			#define DESTINATION_OFFSET (0x1900)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (256)
			#define OUTPUT_CHANNELS    (256)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (64)
			#define WEIGHT_OFFSET_Y    (256)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (1)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.lateral_conv0
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1900)
			#define DESTINATION_OFFSET (0x1580)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (256)
			#define OUTPUT_CHANNELS    (128)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (64)
			#define WEIGHT_OFFSET_Y    (128)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (8)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.upsample
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1580)
			#define DESTINATION_OFFSET (0x1800)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (128)
			#define INPUT_TEXTURE      _GrabTexture
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/UpsampleNearest2x.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.C3_p4.conv1
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1800)
			#define DESTINATION_OFFSET (0x1a00)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (256)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (60)
			#define WEIGHT_OFFSET_Y    (704)
			#define BIAS_OFFSET_X      (32)
			#define BIAS_OFFSET_Y      (23)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		// backbone.C3_p4.conv2
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1800)
			#define DESTINATION_OFFSET (0x1b00)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (256)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (60)
			#define WEIGHT_OFFSET_Y    (672)
			#define BIAS_OFFSET_X      (64)
			#define BIAS_OFFSET_Y      (23)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.C3_p4.m.0.conv1
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1a00)
			#define DESTINATION_OFFSET (0x1a00)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (76)
			#define WEIGHT_OFFSET_Y    (800)
			#define BIAS_OFFSET_X      (96)
			#define BIAS_OFFSET_Y      (23)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.C3_p4.m.0.conv2.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1a00)
			#define DESTINATION_OFFSET (0x1a00)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (57)
			#define WEIGHT_OFFSET_Y    (736)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (22)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3p1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.C3_p4.m.0.conv2.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1a00)
			#define DESTINATION_OFFSET (0x1a00)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (92)
			#define WEIGHT_OFFSET_Y    (800)
			#define BIAS_OFFSET_X      (32)
			#define BIAS_OFFSET_Y      (22)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.C3_p4.conv3
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1a00)
			#define DESTINATION_OFFSET (0x1a00)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (128)
			#define OUTPUT_CHANNELS    (128)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (0)
			#define WEIGHT_OFFSET_Y    (512)
			#define BIAS_OFFSET_X      (64)
			#define BIAS_OFFSET_Y      (8)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.reduce_conv1
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1a00)
			#define DESTINATION_OFFSET (0x1700)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (128)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (0)
			#define WEIGHT_OFFSET_Y    (672)
			#define BIAS_OFFSET_X      (64)
			#define BIAS_OFFSET_Y      (22)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.upsample
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1700)
			#define DESTINATION_OFFSET (0x1800)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (64)
			#define INPUT_TEXTURE      _GrabTexture
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/UpsampleNearest2x.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.C3_p3.conv1
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1800)
			#define DESTINATION_OFFSET (0x1c00)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (128)
			#define OUTPUT_CHANNELS    (32)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (96)
			#define WEIGHT_OFFSET_Y    (864)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (27)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		// backbone.C3_p3.conv2
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1800)
			#define DESTINATION_OFFSET (0x1e00)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (128)
			#define OUTPUT_CHANNELS    (32)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (63)
			#define WEIGHT_OFFSET_Y    (864)
			#define BIAS_OFFSET_X      (48)
			#define BIAS_OFFSET_Y      (27)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.C3_p3.m.0.conv1
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1c00)
			#define DESTINATION_OFFSET (0x1c00)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (32)
			#define OUTPUT_CHANNELS    (32)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (51)
			#define WEIGHT_OFFSET_Y    (784)
			#define BIAS_OFFSET_X      (32)
			#define BIAS_OFFSET_Y      (27)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.C3_p3.m.0.conv2.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1c00)
			#define DESTINATION_OFFSET (0x1c00)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (32)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (48)
			#define WEIGHT_OFFSET_Y    (768)
			#define BIAS_OFFSET_X      (80)
			#define BIAS_OFFSET_Y      (27)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3p1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.C3_p3.m.0.conv2.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1c00)
			#define DESTINATION_OFFSET (0x1c00)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (32)
			#define OUTPUT_CHANNELS    (32)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (51)
			#define WEIGHT_OFFSET_Y    (768)
			#define BIAS_OFFSET_X      (64)
			#define BIAS_OFFSET_Y      (27)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.C3_p3.conv3
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1c00)
			#define DESTINATION_OFFSET (0x1c00)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (16)
			#define WEIGHT_OFFSET_Y    (800)
			#define BIAS_OFFSET_X      (96)
			#define BIAS_OFFSET_Y      (22)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.bu_conv2.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1c00)
			#define DESTINATION_OFFSET (0x1600)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (48)
			#define WEIGHT_OFFSET_Y    (704)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (21)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3s2p1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.bu_conv2.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1600)
			#define DESTINATION_OFFSET (0x1600)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (60)
			#define WEIGHT_OFFSET_Y    (800)
			#define BIAS_OFFSET_X      (32)
			#define BIAS_OFFSET_Y      (21)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.C3_n3.conv1
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1600)
			#define DESTINATION_OFFSET (0x1600)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (128)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (0)
			#define WEIGHT_OFFSET_Y    (608)
			#define BIAS_OFFSET_X      (64)
			#define BIAS_OFFSET_Y      (21)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		// backbone.C3_n3.conv2
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1600)
			#define DESTINATION_OFFSET (0x1700)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (128)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (0)
			#define WEIGHT_OFFSET_Y    (640)
			#define BIAS_OFFSET_X      (96)
			#define BIAS_OFFSET_Y      (21)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.C3_n3.m.0.conv1
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1600)
			#define DESTINATION_OFFSET (0x1600)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (0)
			#define WEIGHT_OFFSET_Y    (768)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (20)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.C3_n3.m.0.conv2.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1600)
			#define DESTINATION_OFFSET (0x1600)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (124)
			#define WEIGHT_OFFSET_Y    (704)
			#define BIAS_OFFSET_X      (32)
			#define BIAS_OFFSET_Y      (20)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3p1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.C3_n3.m.0.conv2.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1600)
			#define DESTINATION_OFFSET (0x1600)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (32)
			#define WEIGHT_OFFSET_Y    (768)
			#define BIAS_OFFSET_X      (64)
			#define BIAS_OFFSET_Y      (20)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.C3_n3.conv3
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1600)
			#define DESTINATION_OFFSET (0x1600)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (128)
			#define OUTPUT_CHANNELS    (128)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (32)
			#define WEIGHT_OFFSET_Y    (512)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (7)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.bu_conv1.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1600)
			#define DESTINATION_OFFSET (0x1500)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (128)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (51)
			#define WEIGHT_OFFSET_Y    (576)
			#define BIAS_OFFSET_X      (64)
			#define BIAS_OFFSET_Y      (7)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3s2p1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.bu_conv1.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1500)
			#define DESTINATION_OFFSET (0x1500)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (128)
			#define OUTPUT_CHANNELS    (128)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (96)
			#define WEIGHT_OFFSET_Y    (512)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (6)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.C3_n4.conv1
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1500)
			#define DESTINATION_OFFSET (0x1500)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (256)
			#define OUTPUT_CHANNELS    (128)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (0)
			#define WEIGHT_OFFSET_Y    (384)
			#define BIAS_OFFSET_X      (64)
			#define BIAS_OFFSET_Y      (6)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		// backbone.C3_n4.conv2
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1500)
			#define DESTINATION_OFFSET (0x1580)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (256)
			#define OUTPUT_CHANNELS    (128)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (64)
			#define WEIGHT_OFFSET_Y    (384)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (5)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.C3_n4.m.0.conv1
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1500)
			#define DESTINATION_OFFSET (0x1500)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (128)
			#define OUTPUT_CHANNELS    (128)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (64)
			#define WEIGHT_OFFSET_Y    (512)
			#define BIAS_OFFSET_X      (64)
			#define BIAS_OFFSET_Y      (5)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.C3_n4.m.0.conv2.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1500)
			#define DESTINATION_OFFSET (0x1500)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (128)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (48)
			#define WEIGHT_OFFSET_Y    (576)
			#define BIAS_OFFSET_X      (64)
			#define BIAS_OFFSET_Y      (4)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3p1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.C3_n4.m.0.conv2.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1500)
			#define DESTINATION_OFFSET (0x1500)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (128)
			#define OUTPUT_CHANNELS    (128)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (32)
			#define WEIGHT_OFFSET_Y    (256)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (4)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// backbone.C3_n4.conv3
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1500)
			#define DESTINATION_OFFSET (0x1500)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (256)
			#define OUTPUT_CHANNELS    (256)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (0)
			#define WEIGHT_OFFSET_Y    (128)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (0)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// head.stems.0
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1c00)
			#define DESTINATION_OFFSET (0x1c00)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (32)
			#define WEIGHT_OFFSET_Y    (640)
			#define BIAS_OFFSET_X      (64)
			#define BIAS_OFFSET_Y      (14)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		// head.stems.1
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1600)
			#define DESTINATION_OFFSET (0x1700)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (128)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (0)
			#define WEIGHT_OFFSET_Y    (576)
			#define BIAS_OFFSET_X      (32)
			#define BIAS_OFFSET_Y      (13)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		// head.stems.2
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1500)
			#define DESTINATION_OFFSET (0x15c0)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (256)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (60)
			#define WEIGHT_OFFSET_Y    (640)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (13)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// head.reg_convs.0.0.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1c00)
			#define DESTINATION_OFFSET (0x1c00)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (54)
			#define WEIGHT_OFFSET_Y    (672)
			#define BIAS_OFFSET_X      (96)
			#define BIAS_OFFSET_Y      (17)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3p1.cginc"
			ENDHLSL
		}

		// head.cls_convs.0.0.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1c00)
			#define DESTINATION_OFFSET (0x1800)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (51)
			#define WEIGHT_OFFSET_Y    (704)
			#define BIAS_OFFSET_X      (96)
			#define BIAS_OFFSET_Y      (20)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3p1.cginc"
			ENDHLSL
		}

		// head.reg_convs.1.0.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1700)
			#define DESTINATION_OFFSET (0x1700)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (60)
			#define WEIGHT_OFFSET_Y    (608)
			#define BIAS_OFFSET_X      (96)
			#define BIAS_OFFSET_Y      (16)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3p1.cginc"
			ENDHLSL
		}

		// head.cls_convs.1.0.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1700)
			#define DESTINATION_OFFSET (0x1600)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (57)
			#define WEIGHT_OFFSET_Y    (704)
			#define BIAS_OFFSET_X      (96)
			#define BIAS_OFFSET_Y      (19)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3p1.cginc"
			ENDHLSL
		}

		// head.reg_convs.2.0.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x15c0)
			#define DESTINATION_OFFSET (0x15c0)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (124)
			#define WEIGHT_OFFSET_Y    (640)
			#define BIAS_OFFSET_X      (96)
			#define BIAS_OFFSET_Y      (15)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3p1.cginc"
			ENDHLSL
		}

		// head.cls_convs.2.0.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x15c0)
			#define DESTINATION_OFFSET (0x1580)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (124)
			#define WEIGHT_OFFSET_Y    (672)
			#define BIAS_OFFSET_X      (96)
			#define BIAS_OFFSET_Y      (18)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3p1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// head.reg_convs.0.0.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1c00)
			#define DESTINATION_OFFSET (0x1c00)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (0)
			#define WEIGHT_OFFSET_Y    (736)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (16)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		// head.cls_convs.0.0.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1800)
			#define DESTINATION_OFFSET (0x1800)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (108)
			#define WEIGHT_OFFSET_Y    (768)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (19)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		// head.reg_convs.1.0.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1700)
			#define DESTINATION_OFFSET (0x1700)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (108)
			#define WEIGHT_OFFSET_Y    (736)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (15)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		// head.cls_convs.1.0.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1600)
			#define DESTINATION_OFFSET (0x1600)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (92)
			#define WEIGHT_OFFSET_Y    (768)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (18)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		// head.reg_convs.2.0.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x15c0)
			#define DESTINATION_OFFSET (0x15c0)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (32)
			#define WEIGHT_OFFSET_Y    (704)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (14)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		// head.cls_convs.2.0.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1580)
			#define DESTINATION_OFFSET (0x1580)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (60)
			#define WEIGHT_OFFSET_Y    (768)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (17)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// head.reg_convs.0.1.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1c00)
			#define DESTINATION_OFFSET (0x1c00)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (57)
			#define WEIGHT_OFFSET_Y    (672)
			#define BIAS_OFFSET_X      (32)
			#define BIAS_OFFSET_Y      (16)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3p1.cginc"
			ENDHLSL
		}

		// head.cls_convs.0.1.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1800)
			#define DESTINATION_OFFSET (0x1800)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (54)
			#define WEIGHT_OFFSET_Y    (704)
			#define BIAS_OFFSET_X      (32)
			#define BIAS_OFFSET_Y      (19)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3p1.cginc"
			ENDHLSL
		}

		// head.reg_convs.1.1.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1700)
			#define DESTINATION_OFFSET (0x1700)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (48)
			#define WEIGHT_OFFSET_Y    (640)
			#define BIAS_OFFSET_X      (32)
			#define BIAS_OFFSET_Y      (15)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3p1.cginc"
			ENDHLSL
		}

		// head.cls_convs.1.1.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1600)
			#define DESTINATION_OFFSET (0x1600)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (48)
			#define WEIGHT_OFFSET_Y    (672)
			#define BIAS_OFFSET_X      (32)
			#define BIAS_OFFSET_Y      (18)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3p1.cginc"
			ENDHLSL
		}

		// head.reg_convs.2.1.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x15c0)
			#define DESTINATION_OFFSET (0x15c0)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (60)
			#define WEIGHT_OFFSET_Y    (576)
			#define BIAS_OFFSET_X      (32)
			#define BIAS_OFFSET_Y      (14)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3p1.cginc"
			ENDHLSL
		}

		// head.cls_convs.2.1.dconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1580)
			#define DESTINATION_OFFSET (0x1580)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (51)
			#define WEIGHT_OFFSET_Y    (672)
			#define BIAS_OFFSET_X      (32)
			#define BIAS_OFFSET_Y      (17)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/DepthwiseConvolution3x3p1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// head.reg_convs.0.1.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1c00)
			#define DESTINATION_OFFSET (0x1c00)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (32)
			#define WEIGHT_OFFSET_Y    (736)
			#define BIAS_OFFSET_X      (64)
			#define BIAS_OFFSET_Y      (16)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		// head.cls_convs.0.1.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1800)
			#define DESTINATION_OFFSET (0x1800)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (76)
			#define WEIGHT_OFFSET_Y    (768)
			#define BIAS_OFFSET_X      (64)
			#define BIAS_OFFSET_Y      (19)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		// head.reg_convs.1.1.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1700)
			#define DESTINATION_OFFSET (0x1700)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (92)
			#define WEIGHT_OFFSET_Y    (736)
			#define BIAS_OFFSET_X      (64)
			#define BIAS_OFFSET_Y      (15)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		// head.cls_convs.1.1.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1600)
			#define DESTINATION_OFFSET (0x1600)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (16)
			#define WEIGHT_OFFSET_Y    (768)
			#define BIAS_OFFSET_X      (64)
			#define BIAS_OFFSET_Y      (18)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		// head.reg_convs.2.1.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x15c0)
			#define DESTINATION_OFFSET (0x15c0)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (32)
			#define WEIGHT_OFFSET_Y    (672)
			#define BIAS_OFFSET_X      (96)
			#define BIAS_OFFSET_Y      (14)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		// head.cls_convs.2.1.pconv
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x1580)
			#define DESTINATION_OFFSET (0x1580)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (64)
			#define OUTPUT_CHANNELS    (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define WEIGHT_TEXTURE     _Weights
			#define BIAS_TEXTURE       _Biases
			#define WEIGHT_OFFSET_X    (16)
			#define WEIGHT_OFFSET_Y    (736)
			#define BIAS_OFFSET_X      (64)
			#define BIAS_OFFSET_Y      (17)
			#include "Config.cginc"
			#include "../Common/Activations/SiLU.cginc"
			#include "../Common/Convolution1x1.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// head.*_preds.0
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define REG_SOURCE_OFFSET  (0x1c00)
			#define CLS_SOURCE_OFFSET  (0x1800)
			#define DESTINATION_OFFSET (0x0000)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define INPUT_CHANNELS     (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define PARAMETER_TEXTURE  _Predictor
			#define WEIGHT_OFFSET_X    (0)
			#define WEIGHT_OFFSET_Y    (2)
			#define BIAS_OFFSET_X      (2)
			#define BIAS_OFFSET_Y      (3)
			#include "Config.cginc"
			#include "Prediction.cginc"
			ENDHLSL
		}

		// head.*_preds.1
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define REG_SOURCE_OFFSET  (0x1700)
			#define CLS_SOURCE_OFFSET  (0x1600)
			#define DESTINATION_OFFSET (0x0080)
			#define INPUT_WIDTH        (30)
			#define INPUT_HEIGHT       (16)
			#define INPUT_CHANNELS     (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define PARAMETER_TEXTURE  _Predictor
			#define WEIGHT_OFFSET_X    (0)
			#define WEIGHT_OFFSET_Y    (1)
			#define BIAS_OFFSET_X      (1)
			#define BIAS_OFFSET_Y      (3)
			#include "Config.cginc"
			#include "Prediction.cginc"
			ENDHLSL
		}

		// head.*_preds.2
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define REG_SOURCE_OFFSET  (0x15c0)
			#define CLS_SOURCE_OFFSET  (0x1580)
			#define DESTINATION_OFFSET (0x00a0)
			#define INPUT_WIDTH        (15)
			#define INPUT_HEIGHT       (8)
			#define INPUT_CHANNELS     (64)
			#define INPUT_TEXTURE      _GrabTexture
			#define PARAMETER_TEXTURE  _Predictor
			#define WEIGHT_OFFSET_X    (0)
			#define WEIGHT_OFFSET_Y    (0)
			#define BIAS_OFFSET_X      (0)
			#define BIAS_OFFSET_Y      (3)
			#include "Config.cginc"
			#include "Prediction.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// nms.map
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define DESTINATION_OFFSET (0x0000)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define STRIDE             (8)
			#define INPUT_SIZE         (0x00a8)
			#define INPUT_TEXTURE      _GrabTexture
			#include "Config.cginc"
			#include "ApproximateNMSMap.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// nms.reduce
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define DESTINATION_OFFSET (0x0f00)
			#define INPUT_SIZE         (0x00a8)
			#define INPUT_TEXTURE      _GrabTexture
			#include "Config.cginc"
			#include "ApproximateNMSReduce.cginc"
			ENDHLSL
		}

		// unproject
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define SOURCE_OFFSET      (0x0000)
			#define DESTINATION_OFFSET (0x0e00)
			#define INPUT_SIZE         (0x00a8)
			#define INPUT_WIDTH        (60)
			#define INPUT_HEIGHT       (32)
			#define STRIDE             (8)
			#define IMAGE_WIDTH        (512)
			#define IMAGE_HEIGHT       (288)
			#define INPUT_TEXTURE      _GrabTexture
			#define DEPTH_TEXTURE      _DepthTex
			#include "Config.cginc"
			#include "Unproject.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// face_selector.map
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define DESTINATION_OFFSET  (0x0d00)
			#define SOURCE_BOX_OFFSET   (0x0f00)
			#define SOURCE_POINT_OFFSET (0x0e00)
			#define INPUT_WIDTH         (60)
			#define INPUT_HEIGHT        (32)
			#define STRIDE              (8)
			#define INPUT_SIZE          (0x00a8)
			#define OUTPUT_SIZE         (0x0004)
			#define IMAGE_WIDTH         (512)
			#define IMAGE_HEIGHT        (288)
			#define INPUT_TEXTURE       _GrabTexture
			#include "Config.cginc"
			#include "FaceSelectorMap.cginc"
			ENDHLSL
		}

		// pixel_selector.map
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define DESTINATION_OFFSET  (0x1000)
			#define INPUT_WIDTH         (1024)
			#define INPUT_HEIGHT        (576)
			#define OUTPUT_WIDTH        (256)
			#define OUTPUT_HEIGHT       (128)
			#define INV_SCALE           (4)
			#define INPUT_TEXTURE       _GrabTexture
			#define DEPTH_TEXTURE       _DepthTex
			#include "Config.cginc"
			#include "PixelSelectorMap.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// face_selector.reduce
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define DESTINATION_OFFSET  (0x0000)
			#define SOURCE_OFFSET       (0x0d00)
			#define SOURCE_BOX_OFFSET   (0x0f00)
			#define SOURCE_POINT_OFFSET (0x0e00)
			#define INPUT_WIDTH         (60)
			#define INPUT_HEIGHT        (32)
			#define STRIDE              (8)
			#define INPUT_SIZE          (0x0004)
			#define IMAGE_WIDTH         (512)
			#define IMAGE_HEIGHT        (288)
			#define INPUT_TEXTURE       _GrabTexture
			#include "Config.cginc"
			#include "FaceSelectorReduce.cginc"
			ENDHLSL
		}

		// pixel_selector.reduce1
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define DESTINATION_OFFSET  (0x1000)
			#define SOURCE_OFFSET       (0x1000)
			#define INPUT_SIZE          (0x0800)
			#define OUTPUT_SIZE         (0x0080)
			#define INPUT_TEXTURE       _GrabTexture
			#include "Config.cginc"
			#include "PixelSelectorReduce.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// eye_selector.map
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define DESTINATION_OFFSET  (0x0c00)
			#define SOURCE_BOX_OFFSET   (0x0f00)
			#define SOURCE_POINT_OFFSET (0x0e00)
			#define INPUT_WIDTH         (60)
			#define INPUT_HEIGHT        (32)
			#define STRIDE              (8)
			#define INPUT_SIZE          (0x00a8)
			#define OUTPUT_SIZE         (0x0004)
			#define IMAGE_WIDTH         (512)
			#define IMAGE_HEIGHT        (288)
			#define INPUT_TEXTURE       _GrabTexture
			#include "Config.cginc"
			#include "EyeSelectorMap.cginc"
			ENDHLSL
		}

		// pixel_selector.reduce2
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define DESTINATION_OFFSET  (0x1000)
			#define SOURCE_OFFSET       (0x1000)
			#define INPUT_SIZE          (0x0080)
			#define OUTPUT_SIZE         (0x0004)
			#define INPUT_TEXTURE       _GrabTexture
			#include "Config.cginc"
			#include "PixelSelectorReduce.cginc"
			ENDHLSL
		}

		GrabPass {
			Tags { "LightMode" = "Vertex" }
		}

		// eye_selector.reduce
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define DESTINATION_OFFSET  (0x0000)
			#define SOURCE_OFFSET       (0x0c00)
			#define SOURCE_BOX_OFFSET   (0x0f00)
			#define SOURCE_POINT_OFFSET (0x0e00)
			#define INPUT_WIDTH         (60)
			#define INPUT_HEIGHT        (32)
			#define STRIDE              (8)
			#define INPUT_SIZE          (0x0004)
			#define IMAGE_WIDTH         (512)
			#define IMAGE_HEIGHT        (288)
			#define INPUT_TEXTURE       _GrabTexture
			#include "Config.cginc"
			#include "EyeSelectorReduce.cginc"
			ENDHLSL
		}

		// pixel_selector.reduce3
		Pass
		{
			Tags { "LightMode" = "Vertex" }
			HLSLPROGRAM
			#pragma target   5.0
			#pragma vertex   vertex
			#pragma fragment fragment
			#define DESTINATION_OFFSET  (0x1000)
			#define SOURCE_OFFSET       (0x1000)
			#define INPUT_SIZE          (0x0004)
			#define INPUT_TEXTURE       _GrabTexture
			#include "Config.cginc"
			#include "PixelSelectorReduceLast.cginc"
			ENDHLSL
		}
	}
}
