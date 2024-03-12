#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using XWear.IO.XResource.Texture;

namespace XWear.IO.Editor
{
    public static class EditorAssetUtil
    {
        private static readonly Dictionary<string, string> BuiltInAssetCache =
            new Dictionary<string, string>();

        private static string GetTmpDir(string subDir = "")
        {
            var dirPath = Path.Combine("Assets", "_XWearTmp");
            if (!string.IsNullOrEmpty(subDir))
            {
                dirPath = Path.Combine(dirPath, subDir);
            }

            return dirPath;
        }

        public static string CopyAssetToTempDir(
            UnityEngine.Object asset,
            string subDir = "",
            bool useGuidFileName = false
        )
        {
            // Unityのビルトインテクスチャやマテリアルが含まれている場合がある
            // ビルトインアセットはそのままだとAssetDatabase.GetAssetPath()で正常にパスを得られないため、
            // 判定にかけてからアセットの実体をUnityEditor.AssetDatabase.GetBuiltinExtraResource()で得る
            if (UnityBuiltInAssetUtil.IsBuiltInAsset(asset.name))
            {
                return CopyBuiltInAsset(asset, subDir);
            }

            var sourceAssetPath = AssetDatabase.GetAssetPath(asset);

            // 一時アセットを作って再インポート
            var sourceFileNameWithoutExtension = useGuidFileName
                ? System.Guid.NewGuid().ToString()
                : Path.GetFileNameWithoutExtension(sourceAssetPath);

            var sourceFileNameExtension = Path.GetExtension(sourceAssetPath);

            if (!Directory.Exists(GetTmpDir(subDir)))
            {
                Directory.CreateDirectory(GetTmpDir(subDir));
            }

            var tmpAssetPath = Path.Combine(
                GetTmpDir(subDir),
                $"{sourceFileNameWithoutExtension}{sourceFileNameExtension}"
            );
            var uniqueTmpAssetPath = AssetDatabase.GenerateUniqueAssetPath(tmpAssetPath);

            AssetDatabase.CopyAsset(sourceAssetPath, uniqueTmpAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return uniqueTmpAssetPath;
        }

        private static string CopyBuiltInAsset(UnityEngine.Object asset, string subDir)
        {
            if (BuiltInAssetCache.TryGetValue(asset.name, out var cached))
            {
                return cached;
            }

            // Unityのビルトインテクスチャやマテリアルが含まれている場合がある
            // ビルトインアセットはそのままだとAssetDatabase.GetAssetPath()で正常にパスを得られないため、
            // 判定にかけてからアセットの実体をUnityEditor.AssetDatabase.GetBuiltinExtraResource()で得たのち、インスタンス化する

            if (asset is UnityEngine.Material)
            {
                if (
                    UnityBuiltInAssetUtil.GetBuiltInMaterial(
                        asset.name,
                        out var builtInAsset,
                        out var ext
                    )
                )
                {
                    var copied = UnityEngine.Object.Instantiate(builtInAsset);
                    copied.name = builtInAsset.name;

                    var tmpAssetPath = Path.Combine(GetTmpDir(subDir), $"{asset.name}{ext}");

                    var uniqueTmpAssetPath = AssetDatabase.GenerateUniqueAssetPath(tmpAssetPath);
                    AssetDatabase.CreateAsset(copied, uniqueTmpAssetPath);
                    BuiltInAssetCache.Add(asset.name, uniqueTmpAssetPath);

                    // アセットとして保存したのでコピー元は破棄してよい
                    UnityEngine.Object.DestroyImmediate(copied);

                    return uniqueTmpAssetPath;
                }
            }

            if (asset is UnityEngine.Texture2D)
            {
                if (
                    UnityBuiltInAssetUtil.GetBuiltInTexture(asset.name, out var builtInAsset, out _)
                )
                {
                    UnityEngine.Texture2D cloned;
                    if (builtInAsset is UnityEngine.Sprite sprite)
                    {
                        cloned = sprite.texture.CloneTexture2D();
                    }
                    else if (builtInAsset is UnityEngine.Texture2D texture2d)
                    {
                        cloned = texture2d.CloneTexture2D();
                    }
                    else
                    {
                        throw new InvalidDataException(
                            $"対応していないビルトインアセット:{builtInAsset.name}_{builtInAsset.GetType()}"
                        );
                    }

                    var clonedTextureByte = UnityEngine.ImageConversion.EncodeToPNG(cloned);
                    UnityEngine.Object.DestroyImmediate(cloned);

                    var ext = ".png";
                    var tmpAssetPath = Path.Combine(GetTmpDir(subDir), $"{asset.name}{ext}");
                    var uniqueTmpAssetPath = AssetDatabase.GenerateUniqueAssetPath(tmpAssetPath);
                    File.WriteAllBytes(uniqueTmpAssetPath, clonedTextureByte);
                    AssetDatabase.Refresh();
                    BuiltInAssetCache.Add(asset.name, uniqueTmpAssetPath);

                    return uniqueTmpAssetPath;
                }
            }

            throw new Exception($"対応していないビルトインアセットが含まれています:{asset.name}");
        }

        public static void Cleanup()
        {
            if (!Directory.Exists(GetTmpDir()))
            {
                return;
            }

            Directory.Delete(GetTmpDir(), recursive: true);

            var metaFile = GetTmpDir() + ".meta";
            if (File.Exists(metaFile))
            {
                File.Delete(metaFile);
            }

            AssetDatabase.Refresh();
            BuiltInAssetCache.Clear();
        }
    }
}
#endif
