#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace XWear.IO.Editor
{
    public static class EditorTextureUtil
    {
        public static UnityEngine.Texture CopyTextureFromAsset(
            UnityEngine.Texture textureAsset,
            string subDir = ""
        )
        {
            var uniqueTmpAssetPath = EditorAssetUtil.CopyAssetToTempDir(textureAsset, subDir);
            var textureImporter = AssetImporter.GetAtPath(uniqueTmpAssetPath) as TextureImporter;
            if (textureImporter == null)
            {
                throw new Exception($"Textureの複製に失敗 {uniqueTmpAssetPath}");
            }

            textureImporter.isReadable = true;
            textureImporter.SaveAndReimport();
            var tmpTextureAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Texture>(
                uniqueTmpAssetPath
            );

            return tmpTextureAsset;
        }
    }
}
#endif
