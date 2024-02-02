#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace lilToon
{
    public class GalshopTomngo_MultiMaskTransparent : lilToonInspector
    {
        // Custom properties
        MaterialProperty alphaMask2;
        MaterialProperty alphaMaskValueG;
        MaterialProperty alphaMaskValueB;
        MaterialProperty alphaMaskValueA;
        MaterialProperty alphaMaskValue2;
        MaterialProperty alphaMaskValue2G;
        MaterialProperty alphaMaskValue2B;
        MaterialProperty alphaMaskValue2A;

        MaterialProperty alphaMaskScale;
        MaterialProperty alphaMaskValue;

        private static bool isShowCustomProperties;
        private const string shaderName = "GalshopTomngo/lilToon_MultiMaskTransparent";

        protected override void LoadCustomProperties(MaterialProperty[] props, Material material)
        {
            isCustomShader = true;

            // If you want to change rendering modes in the editor, specify the shader here
            ReplaceToCustomShaders();
            isShowRenderMode = !material.shader.name.Contains("Optional");

            // If not, set isShowRenderMode to false
            //isShowRenderMode = false;

            //LoadCustomLanguage("");
            alphaMask2 = FindProperty("_AlphaMask2", props);
            alphaMaskValueG = FindProperty("_AlphaMaskValueG", props);
            alphaMaskValueB = FindProperty("_AlphaMaskValueB", props);
            alphaMaskValueA = FindProperty("_AlphaMaskValueA", props);
            alphaMaskValue2 = FindProperty("_AlphaMaskValue2", props);
            alphaMaskValue2G = FindProperty("_AlphaMaskValue2G", props);
            alphaMaskValue2B = FindProperty("_AlphaMaskValue2B", props);
            alphaMaskValue2A = FindProperty("_AlphaMaskValue2A", props);

            alphaMaskScale = FindProperty("_AlphaMaskScale", props);
            alphaMaskValue = FindProperty("_AlphaMaskValue", props);
        }

        protected override void DrawCustomProperties(Material material)
        {
            // GUIStyles Name   Description
            // ---------------- ------------------------------------
            // boxOuter         outer box
            // boxInnerHalf     inner box
            // boxInner         inner box without label
            // customBox        box (similar to unity default box)
            // customToggleFont label for box
            isShowCustomProperties = Foldout("Custom Properties", "Custom Properties", isShowCustomProperties);
            if (isShowCustomProperties)
            {
                //EditorGUILayout.BeginVertical(boxOuter);
                //EditorGUILayout.LabelField(GetLoc("Custom Properties"), customToggleFont);
                //EditorGUILayout.BeginVertical(boxInnerHalf);

                bool invertAlphaMask = alphaMaskScale.floatValue < 0;
                float transparencyG = alphaMaskValueG.floatValue - (invertAlphaMask ? 1.0f : 0.0f);
                float transparencyB = alphaMaskValueB.floatValue - (invertAlphaMask ? 1.0f : 0.0f);
                float transparencyA = alphaMaskValueA.floatValue - (invertAlphaMask ? 1.0f : 0.0f);
                float transparency2 = alphaMaskValue2.floatValue - (invertAlphaMask ? 1.0f : 0.0f);
                float transparency2G = alphaMaskValue2G.floatValue - (invertAlphaMask ? 1.0f : 0.0f);
                float transparency2B = alphaMaskValue2B.floatValue - (invertAlphaMask ? 1.0f : 0.0f);
                float transparency2A = alphaMaskValue2A.floatValue - (invertAlphaMask ? 1.0f : 0.0f);

                GUIContent alphaMask2Label = new GUIContent("Alpha Mask 2");
                m_MaterialEditor.TexturePropertySingleLine(alphaMask2Label, alphaMask2);

                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = alphaMaskScale.hasMixedValue || alphaMaskValue.hasMixedValue;
                transparencyG = EditorGUILayout.Slider("Transparency(G)", transparencyG, 0.0f, 1.0f);
                transparencyB = EditorGUILayout.Slider("Transparency(B)", transparencyB, 0.0f, 1.0f);
                transparencyA = EditorGUILayout.Slider("Transparency(A)", transparencyA, 0.0f, 1.0f);
                transparency2 = EditorGUILayout.Slider("Transparency2(R)", transparency2, 0.0f, 1.0f);
                transparency2G = EditorGUILayout.Slider("Transparency2(G)", transparency2G, 0.0f, 1.0f);
                transparency2B = EditorGUILayout.Slider("Transparency2(B)", transparency2B, 0.0f, 1.0f);
                transparency2A = EditorGUILayout.Slider("Transparency2(A)", transparency2A, 0.0f, 1.0f);
                EditorGUI.showMixedValue = false;

                if (EditorGUI.EndChangeCheck())
                {
                    alphaMaskValueG.floatValue = transparencyG + (invertAlphaMask ? 1.0f : 0.0f);
                    alphaMaskValueB.floatValue = transparencyB + (invertAlphaMask ? 1.0f : 0.0f);
                    alphaMaskValueA.floatValue = transparencyA + (invertAlphaMask ? 1.0f : 0.0f);
                    alphaMaskValue2.floatValue = transparency2 + (invertAlphaMask ? 1.0f : 0.0f);
                    alphaMaskValue2G.floatValue = transparency2G + (invertAlphaMask ? 1.0f : 0.0f);
                    alphaMaskValue2B.floatValue = transparency2B + (invertAlphaMask ? 1.0f : 0.0f);
                    alphaMaskValue2A.floatValue = transparency2A + (invertAlphaMask ? 1.0f : 0.0f);
                }

                //m_MaterialEditor.ShaderProperty(alphaMaskValueG, "_AlphaMaskValueG");
                //m_MaterialEditor.ShaderProperty(alphaMaskValueB, "_AlphaMaskValueB");
                //m_MaterialEditor.ShaderProperty(alphaMaskValueA, "_AlphaMaskValueA");
                //m_MaterialEditor.ShaderProperty(alphaMaskValue2, "_AlphaMaskValue2");
                //m_MaterialEditor.ShaderProperty(alphaMaskValue2G, "_AlphaMaskValue2G");
                //m_MaterialEditor.ShaderProperty(alphaMaskValue2B, "_AlphaMaskValue2B");
                //m_MaterialEditor.ShaderProperty(alphaMaskValue2A, "_AlphaMaskValue2A");
                //EditorGUILayout.EndVertical();
                //EditorGUILayout.EndVertical();

            }
        }

        protected override void ReplaceToCustomShaders()
        {
            lts         = Shader.Find(shaderName + "/lilToon");
            ltsc        = Shader.Find("Hidden/" + shaderName + "/Cutout");
            ltst        = Shader.Find("Hidden/" + shaderName + "/Transparent");
            ltsot       = Shader.Find("Hidden/" + shaderName + "/OnePassTransparent");
            ltstt       = Shader.Find("Hidden/" + shaderName + "/TwoPassTransparent");

            ltso        = Shader.Find("Hidden/" + shaderName + "/OpaqueOutline");
            ltsco       = Shader.Find("Hidden/" + shaderName + "/CutoutOutline");
            ltsto       = Shader.Find("Hidden/" + shaderName + "/TransparentOutline");
            ltsoto      = Shader.Find("Hidden/" + shaderName + "/OnePassTransparentOutline");
            ltstto      = Shader.Find("Hidden/" + shaderName + "/TwoPassTransparentOutline");

            ltsoo       = Shader.Find(shaderName + "/[Optional] OutlineOnly/Opaque");
            ltscoo      = Shader.Find(shaderName + "/[Optional] OutlineOnly/Cutout");
            ltstoo      = Shader.Find(shaderName + "/[Optional] OutlineOnly/Transparent");

            ltstess     = Shader.Find("Hidden/" + shaderName + "/Tessellation/Opaque");
            ltstessc    = Shader.Find("Hidden/" + shaderName + "/Tessellation/Cutout");
            ltstesst    = Shader.Find("Hidden/" + shaderName + "/Tessellation/Transparent");
            ltstessot   = Shader.Find("Hidden/" + shaderName + "/Tessellation/OnePassTransparent");
            ltstesstt   = Shader.Find("Hidden/" + shaderName + "/Tessellation/TwoPassTransparent");

            ltstesso    = Shader.Find("Hidden/" + shaderName + "/Tessellation/OpaqueOutline");
            ltstessco   = Shader.Find("Hidden/" + shaderName + "/Tessellation/CutoutOutline");
            ltstessto   = Shader.Find("Hidden/" + shaderName + "/Tessellation/TransparentOutline");
            ltstessoto  = Shader.Find("Hidden/" + shaderName + "/Tessellation/OnePassTransparentOutline");
            ltstesstto  = Shader.Find("Hidden/" + shaderName + "/Tessellation/TwoPassTransparentOutline");

            ltsl        = Shader.Find(shaderName + "/lilToonLite");
            ltslc       = Shader.Find("Hidden/" + shaderName + "/Lite/Cutout");
            ltslt       = Shader.Find("Hidden/" + shaderName + "/Lite/Transparent");
            ltslot      = Shader.Find("Hidden/" + shaderName + "/Lite/OnePassTransparent");
            ltsltt      = Shader.Find("Hidden/" + shaderName + "/Lite/TwoPassTransparent");

            ltslo       = Shader.Find("Hidden/" + shaderName + "/Lite/OpaqueOutline");
            ltslco      = Shader.Find("Hidden/" + shaderName + "/Lite/CutoutOutline");
            ltslto      = Shader.Find("Hidden/" + shaderName + "/Lite/TransparentOutline");
            ltsloto     = Shader.Find("Hidden/" + shaderName + "/Lite/OnePassTransparentOutline");
            ltsltto     = Shader.Find("Hidden/" + shaderName + "/Lite/TwoPassTransparentOutline");

            ltsref      = Shader.Find("Hidden/" + shaderName + "/Refraction");
            ltsrefb     = Shader.Find("Hidden/" + shaderName + "/RefractionBlur");
            ltsfur      = Shader.Find("Hidden/" + shaderName + "/Fur");
            ltsfurc     = Shader.Find("Hidden/" + shaderName + "/FurCutout");
            ltsfurtwo   = Shader.Find("Hidden/" + shaderName + "/FurTwoPass");
            ltsfuro     = Shader.Find(shaderName + "/[Optional] FurOnly/Transparent");
            ltsfuroc    = Shader.Find(shaderName + "/[Optional] FurOnly/Cutout");
            ltsfurotwo  = Shader.Find(shaderName + "/[Optional] FurOnly/TwoPass");
            ltsgem      = Shader.Find("Hidden/" + shaderName + "/Gem");
            ltsfs       = Shader.Find(shaderName + "/[Optional] FakeShadow");

            ltsover     = Shader.Find(shaderName + "/[Optional] Overlay");
            ltsoover    = Shader.Find(shaderName + "/[Optional] OverlayOnePass");
            ltslover    = Shader.Find(shaderName + "/[Optional] LiteOverlay");
            ltsloover   = Shader.Find(shaderName + "/[Optional] LiteOverlayOnePass");

            ltsm        = Shader.Find(shaderName + "/lilToonMulti");
            ltsmo       = Shader.Find("Hidden/" + shaderName + "/MultiOutline");
            ltsmref     = Shader.Find("Hidden/" + shaderName + "/MultiRefraction");
            ltsmfur     = Shader.Find("Hidden/" + shaderName + "/MultiFur");
            ltsmgem     = Shader.Find("Hidden/" + shaderName + "/MultiGem");
        }

        // You can create a menu like this
        /*
        [MenuItem("Assets/TemplateFull/Convert material to custom shader", false, 1100)]
        private static void ConvertMaterialToCustomShaderMenu()
        {
            if(Selection.objects.Length == 0) return;
            TemplateFullInspector inspector = new TemplateFullInspector();
            for(int i = 0; i < Selection.objects.Length; i++)
            {
                if(Selection.objects[i] is Material)
                {
                    inspector.ConvertMaterialToCustomShader((Material)Selection.objects[i]);
                }
            }
        }
        */
    }
}
#endif