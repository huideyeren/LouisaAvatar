#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using XWear.IO.XResource;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Texture;

namespace XWear.IO.Editor.Texture
{
    public class EditorTextureBuilder : TextureBuilderBase
    {
        private readonly AssetSaver _assetSaver;

        public EditorTextureBuilder(AssetSaver assetSaver)
        {
            _assetSaver = assetSaver;
        }

        public override List<XResourceTextureInstance> BuildXResourceTextures(
            XItem xItem,
            XResourceContainerUtil.XResourceOpener opener
        )
        {
            var result = new List<XResourceTextureInstance>();
            var xResourceTextures = xItem.XResourceTextures;
            foreach (var xResourceTexture in xResourceTextures)
            {
                // 異なるDressPart間で共通のテクスチャが使われている状態があり得るので、重複を考慮する
                if (!GuidToTexture.TryGetValue(xResourceTexture.Guid, out var instance))
                {
                    var textureBinary = opener.GetXResourceTextureBinary(xResourceTexture);
                    var texture = _assetSaver.CreateTextureAsset(
                        xResourceTexture.TextureParam.name,
                        textureBinary,
                        out var assetPath
                    );

                    var textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                    if (textureImporter != null)
                    {
                        if (xResourceTexture.isNormal)
                        {
                            textureImporter.textureType = TextureImporterType.NormalMap;
                        }

                        textureImporter.alphaIsTransparency = xResourceTexture.alphaIsTransparency;
                        var textureImporterSettings = xResourceTexture.TextureImportSettings;
                        textureImporter.streamingMipmaps = textureImporterSettings.streamingMipmaps;
                        textureImporter.mipmapEnabled = textureImporterSettings.mipmapEnabled;
                        textureImporter.crunchedCompression =
                            textureImporterSettings.crunchedCompression;
                        textureImporter.compressionQuality =
                            textureImporterSettings.compressionQuality;
                        textureImporter.maxTextureSize = textureImporterSettings.maxTextureSize;
                        textureImporter.SaveAndReimport();
                    }

                    instance = new XResourceTextureInstance()
                    {
                        Guid = xResourceTexture.Guid,
                        TextureInstance = texture
                    };

                    GuidToTexture.Add(xResourceTexture.Guid, instance);
                }

                result.Add(instance);
            }

            return result;
        }
    }
}
#endif
