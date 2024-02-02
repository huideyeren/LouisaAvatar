/// <summary>
/// Copyright (c) 2020 VRM Consortium Copyright (c) 2018 Masataka SUMI for MToon
/// The following code uses VRMShader's code licensed under the MIT license
/// https://github.com/vrm-c/UniVRM/blob/v0.108.0/Assets/VRMShaders/LICENSE.md
/// original: https://github.com/vrm-c/UniVRM/blob/v0.108.0/Assets/VRMShaders/GLTF/IO/Resources/UniGLTF/NormalMapExporter.shader
/// </summary>

Shader "Hidden/XWear/NormalMapExporter"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;

            fixed4 frag(v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv);

                // Convert from compressed normal value to usual normal value.
                col.xyz = (UnpackNormal(col) + 1) * 0.5;
                col.w = 1;

                return col;
            }
            ENDCG
        }
    }
}