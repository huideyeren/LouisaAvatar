#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Texture;
using XWear.IO.XResource.Texture.Util;

namespace XWear.IO.Editor.Texture
{
    public class EditorTextureCollector : TextureCollectorBase
    {
        protected override XResourceTexture TryCollectAndAdd(UnityEngine.Texture sourceTexture)
        {
            if (TextureToXResourceTextureMemo.TryGetValue(sourceTexture, out var xResourceTexture))
            {
                return xResourceTexture;
            }

            xResourceTexture = new XResourceTexture(Guid.NewGuid().ToString(), sourceTexture);

            var assetPath = AssetDatabase.GetAssetPath(sourceTexture);
            if (!string.IsNullOrEmpty(assetPath))
            {
                var textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (textureImporter != null)
                {
                    if (textureImporter.textureType == TextureImporterType.NormalMap)
                    {
                        xResourceTexture.isNormal = true;
                    }

                    xResourceTexture.alphaIsTransparency = textureImporter.alphaIsTransparency;
                }
            }

            TextureToXResourceTextureMemo.Add(sourceTexture, xResourceTexture);
            return xResourceTexture;
        }

        public override void Archive(XResourceContainerUtil.XResourceArchiver archiver)
        {
            foreach (var textureToXResourceKvp in TextureToXResourceTextureMemo)
            {
                var textureInstance = textureToXResourceKvp.Key;
                var xResourceTexture = textureToXResourceKvp.Value;
                byte[] textureBytes;

                // テクスチャはアセットのインポート設定によってnoReadWriteである可能性があったり、
                // Unityのビルトインテクスチャが利用されている可能性があるので、一旦一時ディレクトリに保存し、
                // アセットのインポート設定をこちら側で操作する
                if (!xResourceTexture.isNormal)
                {
                    // 非ノーマルマップの場合、テクスチャアセットをコピーする
                    var copiedTexture = EditorTextureUtil.CopyTextureFromAsset(textureInstance);
                    textureBytes = TextureUtil.GetTextureBytes(copiedTexture);
                }
                else
                {
                    // ノーマルマップの場合、ShaderをかけてUnpackNormalした中身を利用する
                    textureBytes = NormalConverter.Export(textureInstance);
                }

                archiver.AddXResourceTexture(
                    textureBytes: textureBytes,
                    xResourceTexture: xResourceTexture
                );
            }

            NormalConverter.Cleanup();
        }
    }
}
#endif
