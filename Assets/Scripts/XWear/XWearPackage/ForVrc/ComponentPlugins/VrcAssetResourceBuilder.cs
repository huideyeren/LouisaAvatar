using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;
using XWear.IO.XResource.Animation;
using XWear.IO.XResource.AvatarMeta;

namespace XWear.XWearPackage.ForVrc.ComponentPlugins
{
#if UNITY_EDITOR
    public class VrcAssetResourceBuilder
    {
        private readonly List<VrcAssetResource> _assetResources;
        private readonly List<VrcExpressionsMenuResource> _expressionsMenuResources;
        private readonly List<AnimatorControllerResource> _animatorControllerResources;
        private readonly Dictionary<string, VrcAssetResource> _assetResourceGuidMap;
        private readonly Dictionary<string, string> _guidToAssetPathMap;

        private readonly Dictionary<string, Object> _loadAssetCache =
            new Dictionary<string, Object>();

        public VrcAssetResourceBuilder(
            List<VrcAssetResource> assetResources,
            Dictionary<string, string> guidToAssetPathMap
        )
        {
            _assetResources = assetResources;
            _assetResourceGuidMap = assetResources.ToDictionary(x => x.Guid, x => x);
            _guidToAssetPathMap = guidToAssetPathMap;
            _expressionsMenuResources = assetResources
                .OfType<VrcExpressionsMenuResource>()
                .ToList();

            _animatorControllerResources = assetResources
                .OfType<AnimatorControllerResource>()
                .ToList();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>MainMenu</returns>
        public void BuildVrcExpressionsMenus(out VRCExpressionsMenu mainMenu)
        {
            mainMenu = null;

            foreach (var expressionsMenuResource in _expressionsMenuResources)
            {
                // Guidから紐づくアセットのパスを得る
                if (
                    !_guidToAssetPathMap.TryGetValue(
                        expressionsMenuResource.Guid,
                        out var assetPath
                    )
                )
                {
                    continue;
                }

                // アセットを読む
                var expressionsMenuAsset =
                    UnityEditor.AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(assetPath);
                if (expressionsMenuResource.type == VrcAssetResource.AssetType.ExpressionsMainMenu)
                {
                    mainMenu = expressionsMenuAsset;
                }

                var assetControls = expressionsMenuAsset.controls;
                var resourceControls = expressionsMenuResource.controls;
                if (assetControls.Count != resourceControls.Count)
                {
                    throw new InvalidDataException();
                }

                for (int i = 0; i < assetControls.Count; i++)
                {
                    var resourceControl = resourceControls[i];
                    var subMenuGuid = resourceControl.subMenuGuid;
                    var iconGuid = resourceControl.iconGuid;

                    assetControls[i].subMenu = Load<VRCExpressionsMenu>(subMenuGuid);
                    assetControls[i].icon = Load<UnityEngine.Texture2D>(iconGuid);
                }
            }
        }

        public VRCExpressionParameters BuildVrcExpressionsParameter()
        {
            var expressionsParameterResource = _assetResources.FirstOrDefault(
                x => x.type == VrcAssetResource.AssetType.ExpressionParameters
            );

            return expressionsParameterResource == null
                ? null
                : Load<VRCExpressionParameters>(expressionsParameterResource.Guid);
        }

        public Dictionary<string, T> BuildSimpleAssets<T>(VrcAssetResource.AssetType assetType)
            where T : Object
        {
            var result = new Dictionary<string, T>();
            var targetTypedResources = _assetResources.Where(x => x.type == assetType).ToList();
            foreach (var targetTypedResource in targetTypedResources)
            {
                // Guidから紐づくアセットのパスを得る
                if (!_guidToAssetPathMap.TryGetValue(targetTypedResource.Guid, out var assetPath))
                {
                    continue;
                }

                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
                result.Add(targetTypedResource.Guid, asset);
            }

            return result;
        }

        public Dictionary<string, RuntimeAnimatorController> BuildAnimatorController(
            Dictionary<string, string> guidToAssetPathMap,
            Dictionary<string, AnimationClip> guidToClipAssetMap
        )
        {
            var result = new Dictionary<string, RuntimeAnimatorController>();
            foreach (var animatorControllerResource in _animatorControllerResources)
            {
                if (string.IsNullOrEmpty(animatorControllerResource.Guid))
                {
                    continue;
                }

                // Guidから紐づくアセットのパスを得る
                if (
                    !_guidToAssetPathMap.TryGetValue(
                        animatorControllerResource.Guid,
                        out var assetPath
                    )
                )
                {
                    UnityEngine.Debug.Log($"No Asset {animatorControllerResource.Name}");
                    continue;
                }

                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                    assetPath
                );

                UnityAnimatorControllerUtil.BuildAnimatorControllerResource(
                    asset,
                    animatorControllerResource,
                    guidToAssetPathMap: guidToAssetPathMap,
                    guidToClipAssetMap: guidToClipAssetMap
                );

                result.Add(animatorControllerResource.Guid, asset);
            }

            return result;
        }

        private T Load<T>(string guid)
            where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            if (_guidToAssetPathMap.TryGetValue(guid, out var assetPath))
            {
                if (!_loadAssetCache.TryGetValue(assetPath, out var asset))
                {
                    asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
                    if (asset == null)
                    {
                        return null;
                    }

                    _loadAssetCache.Add(assetPath, asset);
                }

                return (T)asset;
            }

            return null;
        }
    }
#endif
}
