//----------------------------------------------------------------------------------------------------------------------
// Macro

// Custom variables
//#define LIL_CUSTOM_PROPERTIES \
//    float _CustomVariable;
#define LIL_CUSTOM_PROPERTIES

// Custom textures
#define LIL_CUSTOM_TEXTURES

// Add vertex shader input
//#define LIL_REQUIRE_APP_POSITION
//#define LIL_REQUIRE_APP_TEXCOORD0
//#define LIL_REQUIRE_APP_TEXCOORD1
//#define LIL_REQUIRE_APP_TEXCOORD2
//#define LIL_REQUIRE_APP_TEXCOORD3
//#define LIL_REQUIRE_APP_TEXCOORD4
//#define LIL_REQUIRE_APP_TEXCOORD5
//#define LIL_REQUIRE_APP_TEXCOORD6
//#define LIL_REQUIRE_APP_TEXCOORD7
//#define LIL_REQUIRE_APP_COLOR
//#define LIL_REQUIRE_APP_NORMAL
//#define LIL_REQUIRE_APP_TANGENT
//#define LIL_REQUIRE_APP_VERTEXID

// Add vertex shader output
//#define LIL_V2F_FORCE_TEXCOORD0
//#define LIL_V2F_FORCE_TEXCOORD1
//#define LIL_V2F_FORCE_POSITION_OS
//#define LIL_V2F_FORCE_POSITION_WS
//#define LIL_V2F_FORCE_POSITION_SS
//#define LIL_V2F_FORCE_NORMAL
//#define LIL_V2F_FORCE_TANGENT
//#define LIL_V2F_FORCE_BITANGENT
//#define LIL_CUSTOM_V2F_MEMBER(id0,id1,id2,id3,id4,id5,id6,id7)

// Add vertex copy
#define LIL_CUSTOM_VERT_COPY

// Inserting a process into the vertex shader
//#define LIL_CUSTOM_VERTEX_OS
//#define LIL_CUSTOM_VERTEX_WS

// Inserting a process into pixel shader
//#define BEFORE_xx
//#define OVERRIDE_xx

//----------------------------------------------------------------------------------------------------------------------
// Information about variables
//----------------------------------------------------------------------------------------------------------------------
#undef LIL_CUSTOM_PROPERTIES
#define LIL_CUSTOM_PROPERTIES \
    float  _AlphaMaskValueG; \
    float  _AlphaMaskValueB; \
    float  _AlphaMaskValueA; \
    float  _AlphaMaskValue2; \
    float  _AlphaMaskValue2G; \
    float  _AlphaMaskValue2B; \
    float  _AlphaMaskValue2A;

#define LIL_CUSTOM_TEXTURES \
    TEXTURE2D(_AlphaMask2);
//----------------------------------------------------------------------------------------------------------------------
// Vertex shader inputs (appdata structure)
//
// Type     Name                    Description
// -------- ----------------------- --------------------------------------------------------------------
// float4   input.positionOS        POSITION
// float2   input.uv0               TEXCOORD0
// float2   input.uv1               TEXCOORD1
// float2   input.uv2               TEXCOORD2
// float2   input.uv3               TEXCOORD3
// float2   input.uv4               TEXCOORD4
// float2   input.uv5               TEXCOORD5
// float2   input.uv6               TEXCOORD6
// float2   input.uv7               TEXCOORD7
// float4   input.color             COLOR
// float3   input.normalOS          NORMAL
// float4   input.tangentOS         TANGENT
// uint     vertexID                SV_VertexID

//----------------------------------------------------------------------------------------------------------------------
// Vertex shader outputs or pixel shader inputs (v2f structure)
//
// The structure depends on the pass.
// Please check lil_pass_xx.hlsl for details.
//
// Type     Name                    Description
// -------- ----------------------- --------------------------------------------------------------------
// float4   output.positionCS       SV_POSITION
// float2   output.uv01             TEXCOORD0 TEXCOORD1
// float2   output.uv23             TEXCOORD2 TEXCOORD3
// float3   output.positionOS       object space position
// float3   output.positionWS       world space position
// float3   output.normalWS         world space normal
// float4   output.tangentWS        world space tangent

//----------------------------------------------------------------------------------------------------------------------
// Variables commonly used in the forward pass
//
// These are members of `lilFragData fd`
//
// Type     Name                    Description
// -------- ----------------------- --------------------------------------------------------------------
// float4   col                     lit color
// float3   albedo                  unlit color
// float3   emissionColor           color of emission
// -------- ----------------------- --------------------------------------------------------------------
// float3   lightColor              color of light
// float3   indLightColor           color of indirectional light
// float3   addLightColor           color of additional light
// float    attenuation             attenuation of light
// float3   invLighting             saturate((1.0 - lightColor) * sqrt(lightColor));
// -------- ----------------------- --------------------------------------------------------------------
// float2   uv0                     TEXCOORD0
// float2   uv1                     TEXCOORD1
// float2   uv2                     TEXCOORD2
// float2   uv3                     TEXCOORD3
// float2   uvMain                  Main UV
// float2   uvMat                   MatCap UV
// float2   uvRim                   Rim Light UV
// float2   uvPanorama              Panorama UV
// float2   uvScn                   Screen UV
// bool     isRightHand             input.tangentWS.w > 0.0;
// -------- ----------------------- --------------------------------------------------------------------
// float3   positionOS              object space position
// float3   positionWS              world space position
// float4   positionCS              clip space position
// float4   positionSS              screen space position
// float    depth                   distance from camera
// -------- ----------------------- --------------------------------------------------------------------
// float3x3 TBN                     tangent / bitangent / normal matrix
// float3   T                       tangent direction
// float3   B                       bitangent direction
// float3   N                       normal direction
// float3   V                       view direction
// float3   L                       light direction
// float3   origN                   normal direction without normal map
// float3   origL                   light direction without sh light
// float3   headV                   middle view direction of 2 cameras
// float3   reflectionN             normal direction for reflection
// float3   matcapN                 normal direction for reflection for MatCap
// float3   matcap2ndN              normal direction for reflection for MatCap 2nd
// float    facing                  VFACE
// -------- ----------------------- --------------------------------------------------------------------
// float    vl                      dot(viewDirection, lightDirection);
// float    hl                      dot(headDirection, lightDirection);
// float    ln                      dot(lightDirection, normalDirection);
// float    nv                      saturate(dot(normalDirection, viewDirection));
// float    nvabs                   abs(dot(normalDirection, viewDirection));
// -------- ----------------------- --------------------------------------------------------------------
// float4   triMask                 TriMask (for lite version)
// float3   parallaxViewDirection   mul(tbnWS, viewDirection);
// float2   parallaxOffset          parallaxViewDirection.xy / (parallaxViewDirection.z+0.5);
// float    anisotropy              strength of anisotropy
// float    smoothness              smoothness
// float    roughness               roughness
// float    perceptualRoughness     perceptual roughness
// float    shadowmix               this variable is 0 in the shadow area
// float    audioLinkValue          volume acquired by AudioLink
// -------- ----------------------- --------------------------------------------------------------------
// uint     renderingLayers         light layer of object (for URP / HDRP)
// uint     featureFlags            feature flags (for HDRP)
// uint2    tileIndex               tile index (for HDRP)

#define OVERRIDE_ALPHAMASK \
			float4 maskTexCol = LIL_SAMPLE_2D(_AlphaMask, sampler_MainTex, fd.uvMain); \
			float4 maskTexCol2 = LIL_SAMPLE_2D(_AlphaMask2, sampler_MainTex, fd.uvMain); \
            float alphaMask = maskTexCol.r; \
            float alphaMaskG = maskTexCol.g; \
            float alphaMaskB = maskTexCol.b; \
            float alphaMaskA = maskTexCol.a; \
            float alphaMask2 = maskTexCol2.r; \
            float alphaMask2G = maskTexCol2.g; \
            float alphaMask2B = maskTexCol2.b; \
            float alphaMask2A = maskTexCol2.a; \
			alphaMask = saturate(alphaMask * _AlphaMaskScale + _AlphaMaskValue); \
			alphaMaskG = saturate(alphaMaskG * _AlphaMaskScale + _AlphaMaskValueG); \
			alphaMaskB = saturate(alphaMaskB * _AlphaMaskScale + _AlphaMaskValueB); \
			alphaMaskA = saturate(alphaMaskA * _AlphaMaskScale + _AlphaMaskValueA); \
			alphaMask2 = saturate(alphaMask2 * _AlphaMaskScale + _AlphaMaskValue2); \
			alphaMask2G = saturate(alphaMask2G * _AlphaMaskScale + _AlphaMaskValue2G); \
			alphaMask2B = saturate(alphaMask2B * _AlphaMaskScale + _AlphaMaskValue2B); \
			alphaMask2A = saturate(alphaMask2A * _AlphaMaskScale + _AlphaMaskValue2A); \
			float mixedalphaMask1 = min(min(min(alphaMask , alphaMaskG),alphaMaskB),alphaMaskA); \
			float mixedalphaMask2 = min(min(min(alphaMask2 , alphaMask2G),alphaMask2B),alphaMask2A); \
			float mixedalphaMask = min(mixedalphaMask1,mixedalphaMask2); \
            fd.col.a = _AlphaMaskMode == 1 ? mixedalphaMask : fd.col.a * mixedalphaMask;
//fd.col.a = _AlphaMaskMode == 1 ? mixedalphaMask : fd.col.a * mixedalphaMask;

//#define OVERRIDE_ALPHAMASK \
//        if(_AlphaMaskMode) \
//        { \
//			float4 maskTexCol = LIL_SAMPLE_2D(_AlphaMask, sampler_MainTex, fd.uvMain); \
//			float4 maskTexCol2 = LIL_SAMPLE_2D(_AlphaMask2, sampler_MainTex, fd.uvMain); \
//            float alphaMask = maskTexCol.r; \
//            float alphaMaskG = maskTexCol.g; \
//            float alphaMaskB = maskTexCol.b; \
//            float alphaMaskA = maskTexCol.a; \
//            float alphaMask2 = maskTexCol2.r; \
//            float alphaMask2G = maskTexCol2.g; \
//            float alphaMask2B = maskTexCol2.b; \
//            float alphaMask2A = maskTexCol2.a; \
//			alphaMask = saturate(alphaMask * _AlphaMaskScale + _AlphaMaskValue); \
//			alphaMaskG = saturate(alphaMaskG * _AlphaMaskScale + _AlphaMaskValueG); \
//			alphaMaskB = saturate(alphaMaskB * _AlphaMaskScale + _AlphaMaskValueB); \
//			alphaMaskA = saturate(alphaMaskA * _AlphaMaskScale + _AlphaMaskValueA); \
//			alphaMask2 = saturate(alphaMask2 * _AlphaMaskScale + _AlphaMaskValue2); \
//			alphaMask2G = saturate(alphaMask2G * _AlphaMaskScale + _AlphaMaskValue2G); \
//			alphaMask2B = saturate(alphaMask2B * _AlphaMaskScale + _AlphaMaskValue2B); \
//			alphaMask2A = saturate(alphaMask2A * _AlphaMaskScale + _AlphaMaskValue2A); \
//			float mixedalphaMask1 = min(min(min(alphaMask , alphaMaskG),alphaMaskB),alphaMaskA); \
//			float mixedalphaMask2 = min(min(min(alphaMask2 , alphaMask2G),alphaMask2B),alphaMask2A); \
//			float mixedalphaMask = min(mixedalphaMask1,mixedalphaMask2); \
//            fd.col.a = _AlphaMaskMode == 1 ? mixedalphaMask : fd.col.a * mixedalphaMask; \
//        }
