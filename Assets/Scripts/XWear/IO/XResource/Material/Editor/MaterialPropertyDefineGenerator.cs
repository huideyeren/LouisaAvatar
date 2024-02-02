#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

namespace XWear.IO.XResource.Material.Editor
{
    public static class MaterialPropertyDefineGenerator
    {
        [MenuItem("Assets/VRoid/XWear/Generate Material Property Define")]
        private static void Generate()
        {
            var selected = Selection.GetFiltered(
                typeof(UnityEngine.Material),
                SelectionMode.Assets
            );
            if (selected.Length == 0 || selected[0] == null)
            {
                Debug.Log("Selected material is not found.");
                return;
            }

            var targetMaterial = (UnityEngine.Material)selected[0];
            var result = ScriptableObject.CreateInstance<MaterialPropertyDefine>();

            AssetDatabase.CreateAsset(
                result,
                Path.Combine("Assets", $"MaterialPropertyDefine_{result.shaderName}.mat")
            );
            AssetDatabase.SaveAssets();
        }
    }
}

#endif
