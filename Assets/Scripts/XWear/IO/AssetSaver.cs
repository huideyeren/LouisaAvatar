using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XWear.IO.XResource.Animation;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.AvatarMeta;
using Object = UnityEngine.Object;

namespace XWear.IO
{
    public class AssetSaver
    {
        private string _xItemName;
        private string XItemName
        {
            get
            {
                if (string.IsNullOrEmpty(_xItemName))
                {
                    _xItemName = "export";
                }

                return _xItemName;
            }
        }
        private readonly string _rootFolderPath;
        public string RootFolderPath => _rootFolderPath;

        public readonly Dictionary<string, AnimationClip> GuidToAnimationClipAssetMap = new();
        public readonly Dictionary<string, XResourceAnimationClip> GuidToXResourceAnimationClip =
            new();

        public AssetSaver(string rootFolderPath)
        {
            _rootFolderPath = rootFolderPath;
        }

        public void SetXItemName(string name)
        {
            _xItemName = name;
        }

#if UNITY_EDITOR
        public Texture CreateTextureAsset(
            string textureName,
            byte[] textureBinary,
            out string assetPath
        )
        {
            var rawPath = Path.Combine(
                GetValidFolderPath(Path.Combine(_rootFolderPath, $"{XItemName}.Textures")),
                $"{textureName}.png"
            );

            File.WriteAllBytes(rawPath, textureBinary);

            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            assetPath = rawPath.Replace("\\", "/").Replace(Application.dataPath, "Assets");

            var assetObject = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture>(assetPath);

            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            return assetObject;
        }
#endif

        public GameObject CreateAsset(GameObject gameObject)
        {
#if UNITY_EDITOR
            var assetPath = CreateAssetPath(gameObject);
            UnityEditor.PrefabUtility.SaveAsPrefabAsset(gameObject, assetPath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            var assetObject = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            return assetObject;
#else
            return gameObject;
#endif
        }

        public void CreateAnimationClipAssets(XResourceAnimationClip clipResource)
        {
#if UNITY_EDITOR
            var clipInstance = UnityAnimationClipUtil.ConvertToAnimationClip(clipResource);
            var clipAsset = (AnimationClip)CreateAsset(clipInstance);
            var settings = UnityEditor.AnimationUtility.GetAnimationClipSettings(clipAsset);
            settings.loopTime = clipResource.loopTime;
            UnityEditor.AnimationUtility.SetAnimationClipSettings(clipAsset, settings);

            GuidToAnimationClipAssetMap.Add(clipResource.guid, clipAsset);
            GuidToXResourceAnimationClip.Add(clipResource.guid, clipResource);
#endif
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="opener"></param>
        /// <param name="assetResource"></param>
        /// <returns>展開したアセットのパス</returns>
        public string CreateVrcAssets(
            XResourceContainerUtil.XResourceOpener opener,
            VrcAssetResource assetResource
        )
        {
            var assetPath = "";
#if UNITY_EDITOR
            var assetBinary = opener.ExtractVrcAssetResources(assetResource);
            var folder = GetVrcAssetResourceFolderName(assetResource.type);
            var assetRawPath = Path.Combine(
                GetValidFolderPath(Path.Combine(_rootFolderPath, folder)),
                GetVrcResourceFileName(assetResource)
            );

            File.WriteAllBytes(assetRawPath, assetBinary);

            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            assetPath = assetRawPath.Replace("\\", "/").Replace(Application.dataPath, "Assets");
#endif
            return assetPath;
        }

        public Object CreateAsset<T>(T unityObject)
            where T : Object
        {
#if UNITY_EDITOR
            var assetPath = CreateAssetPath(unityObject);
            UnityEditor.AssetDatabase.CreateAsset(unityObject, assetPath);

            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            var assetObject = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
            return assetObject;
#else
            throw new Exception();
#endif
        }

        private string CreateAssetPath(Object unityObject)
        {
            var rawPath = Path.Combine(
                GetFolderName(unityObject),
                $"{unityObject.name}.{GetExtension(unityObject)}"
            );

            var assetPath = rawPath.Replace("\\", "/").Replace(Application.dataPath, "Assets");

#if UNITY_EDITOR
            assetPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(assetPath);
#endif
            return assetPath;
        }

        private string GetExtension(Object unityObject)
        {
            if (unityObject is Texture)
            {
                return "png";
            }

            if (unityObject is Material)
            {
                return "mat";
            }

            if (unityObject is GameObject)
            {
                return "prefab";
            }

            if (unityObject is AnimationClip)
            {
                return "anim";
            }

            return "asset";
        }

        private string GetFolderName(Object unityObject)
        {
            if (unityObject is Material)
            {
                return GetValidFolderPath(Path.Combine(_rootFolderPath, $"{XItemName}.Materials"));
            }

            if (unityObject is Mesh)
            {
                return GetValidFolderPath(Path.Combine(_rootFolderPath, $"{XItemName}.Meshes"));
            }

            if (unityObject is UnityEngine.Avatar)
            {
                return GetValidFolderPath(Path.Combine(_rootFolderPath, $"{XItemName}.Avatar"));
            }

            if (unityObject is AnimationClip)
            {
                return GetValidFolderPath(
                    Path.Combine(_rootFolderPath, $"{XItemName}.Animations", "AnimationClips")
                );
            }

            return _rootFolderPath;
        }

        public string GetVrcAssetResourceFolderName(VrcAssetResource.AssetType assetType)
        {
            switch (assetType)
            {
                case VrcAssetResource.AssetType.ExpressionParameters:
                case VrcAssetResource.AssetType.ExpressionsMainMenu:
                    return $"{XItemName}.Expressions";
                case VrcAssetResource.AssetType.ExpressionsSubMenu:
                    return Path.Combine($"{XItemName}.Expressions", "SubMenu");
                case VrcAssetResource.AssetType.ExpressionsIcon:
                    return Path.Combine($"{XItemName}.Expressions", "Icons");
                case VrcAssetResource.AssetType.AnimatorController:
                    return Path.Combine($"{XItemName}.Animations", "Controllers");
                case VrcAssetResource.AssetType.AvatarMask:
                    return Path.Combine($"{XItemName}.AvatarMasks");
                default:
                    throw new ArgumentOutOfRangeException(nameof(assetType), assetType, null);
            }
        }

        private string GetVrcResourceFileName(VrcAssetResource assetResource)
        {
            var assetType = assetResource.type;
            switch (assetType)
            {
                case VrcAssetResource.AssetType.ExpressionsMainMenu:
                case VrcAssetResource.AssetType.ExpressionsSubMenu:
                case VrcAssetResource.AssetType.ExpressionParameters:
                case VrcAssetResource.AssetType.BlendTree:
                    return $"{assetResource.Name}.asset";
                case VrcAssetResource.AssetType.ExpressionsIcon:
                    return $"{assetResource.Name}.png";
                case VrcAssetResource.AssetType.AnimatorController:
                    return $"{assetResource.Name}.controller";
                case VrcAssetResource.AssetType.AnimationClip:
                    return $"{assetResource.Name}.anim";
                case VrcAssetResource.AssetType.AvatarMask:
                    return $"{assetResource.Name}.mask";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string GetValidFolderPath(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
#endif
            }

            return dirPath;
        }
    }
}
