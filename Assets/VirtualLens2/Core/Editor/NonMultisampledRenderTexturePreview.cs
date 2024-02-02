using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEditor;

// Unity Editor tries to generate preview for render textures with bindMS flag.
// However, shaders to render it don't use Texture2DMS and cause errors.
// This class suppresses these errors by overriding preview for render textures with bindMS.
// Overriding for built-in editor classes is a hack, so it might not work with future versions.

[CustomEditor(typeof(RenderTexture))]
public class NonMultisampledRenderTexturePreview : Editor
{
    private Editor _originalEditor;

    private Editor GetOriginalEditor()
    {
        if (_originalEditor == null)
        {
            var assembly = Assembly.GetAssembly(typeof(Editor));
            var originalType = assembly
                .GetTypes()
                .FirstOrDefault(t => t.FullName == "UnityEditor.RenderTextureEditor");
            _originalEditor = CreateEditor(target, originalType);
        }
        return _originalEditor;
    }

#if UNITY_2019_1_OR_NEWER
    public override UnityEngine.UIElements.VisualElement CreateInspectorGUI()
    {
        var originalEditor = GetOriginalEditor();
        return originalEditor.CreateInspectorGUI();
    }
#endif
    
    public override void DrawPreview(Rect previewArea)
    {
        var originalEditor = GetOriginalEditor();
        originalEditor.DrawPreview(previewArea);
    }

    public override string GetInfoString()
    {
        var originalEditor = GetOriginalEditor();
        return originalEditor.GetInfoString();
    }

    public override GUIContent GetPreviewTitle()
    {
        var originalEditor = GetOriginalEditor();
        return originalEditor.GetPreviewTitle();
    }
    
    public override bool HasPreviewGUI()
    {
        var rt = target as RenderTexture;
        if (rt == null || rt.bindTextureMS) { return false; }
        var originalEditor = GetOriginalEditor();
        return originalEditor.HasPreviewGUI();
    }

    public override void OnInspectorGUI()
    {
        var originalEditor = GetOriginalEditor();
        originalEditor.OnInspectorGUI();
    }

    public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
    {
        var originalEditor = GetOriginalEditor();
        originalEditor.OnInteractivePreviewGUI(r, background);
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        var originalEditor = GetOriginalEditor();
        originalEditor.OnPreviewGUI(r, background);
    }

    public override void OnPreviewSettings()
    {
        var originalEditor = GetOriginalEditor();
        originalEditor.OnPreviewSettings();
    }
    
    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
    {
        var rt = target as RenderTexture;
        if (rt == null || rt.bindTextureMS) { return new Texture2D(width, height); }
        var originalEditor = GetOriginalEditor();
        return originalEditor.RenderStaticPreview(assetPath, subAssets, width, height);
    }
}
