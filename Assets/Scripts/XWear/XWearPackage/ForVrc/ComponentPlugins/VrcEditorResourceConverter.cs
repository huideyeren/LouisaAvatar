using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VRC.SDK3.Avatars.ScriptableObjects;
using XWear.IO.XResource.AvatarMeta;
using XWear.IO.XResource.Archive;
using UnityEngine;

#if UNITY_EDITOR
using System;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using XWear.IO.Editor;
using XWear.IO.XResource.Animation;
#endif

namespace XWear.XWearPackage.ForVrc.ComponentPlugins
{
#if UNITY_EDITOR
    public static class VrcEditorResourceConverter
    {
        public static AnimatorControllerResource CopyAnimatorControllerWithAnimation(
            List<(VrcAssetResource assetResource, string tmpPath)> result,
            Dictionary<AnimationClip, XResourceAnimationClip> clipInstanceToGuid,
            RuntimeAnimatorController sourceAsset
        )
        {
            var copiedPath = EditorAssetUtil.CopyAssetToTempDir(sourceAsset);
            var copiedAsset = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(copiedPath);
            copiedAsset.name = sourceAsset.name;

            var animatorControllerResource = UnityAnimatorControllerUtil.ToAssetResource(
                copiedAsset,
                vrcAssetResourceResult: result,
                clipInstanceToXResourceMap: clipInstanceToGuid
            );
            result.Add((animatorControllerResource, copiedPath));

            return animatorControllerResource;
        }

        public static VrcAssetResource CopyAndAddSimpleAssetResource<T>(
            List<(VrcAssetResource assetResource, string tmpPath)> result,
            T sourceAsset,
            VrcAssetResource.AssetType assetType
        )
            where T : UnityEngine.Object
        {
            var copiedPath = EditorAssetUtil.CopyAssetToTempDir(sourceAsset);
            var copiedAsset = AssetDatabase.LoadAssetAtPath<T>(copiedPath);
            copiedAsset.name = sourceAsset.name;

            var simpleAssetResource = new SimpleVrcAssetResource()
            {
                Guid = Guid.NewGuid().ToString(),
                Name = sourceAsset.name,
                type = assetType
            };
            result.Add((simpleAssetResource, copiedPath));

            return simpleAssetResource;
        }

        public static VrcAssetResource CopyExpressionsMenu(
            List<(VrcAssetResource assetResource, string tmpPath)> result,
            VRCExpressionsMenu expressionsMenu,
            out string copiedPath,
            bool isSubMenu = false
        )
        {
            // ExpressionsMenuをコピー
            copiedPath = EditorAssetUtil.CopyAssetToTempDir(expressionsMenu);
            var copiedExpression = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(copiedPath);
            copiedExpression.name = expressionsMenu.name;

            var expressionsMenuResource = new VrcExpressionsMenuResource()
            {
                Guid = Guid.NewGuid().ToString(),
                Name = expressionsMenu.name,
                type = isSubMenu
                    ? VrcAssetResource.AssetType.ExpressionsSubMenu
                    : VrcAssetResource.AssetType.ExpressionsMainMenu,
            };

            result.Add((expressionsMenuResource, copiedPath));

            // コピー元のExpressionsMenuからサブメニューとアイコンをコピー
            var sourceControls = expressionsMenu.controls;

            var copiedResourcePaths = new (
                string copiedIconPath,
                string iconName,
                string subMenuPath
            )[sourceControls.Count];

            for (var index = 0; index < sourceControls.Count; index++)
            {
                var resourceControl = new VrcExpressionsMenuResource.Control();

                var control = sourceControls[index];

                var copiedIconPath = "";
                var iconName = "";
                if (control.icon != null)
                {
                    copiedIconPath = EditorAssetUtil.CopyAssetToTempDir(control.icon);
                    iconName = control.icon.name;
                    var iconGuid = Guid.NewGuid().ToString();
                    result.Add(
                        (
                            new SimpleVrcAssetResource()
                            {
                                Guid = iconGuid,
                                Name = iconName,
                                type = VrcAssetResource.AssetType.ExpressionsIcon
                            },
                            copiedIconPath
                        )
                    );

                    resourceControl.iconGuid = iconGuid;
                }

                var copiedSubMenuPath = "";
                if (control.subMenu != null)
                {
                    var subMenuResource = CopyExpressionsMenu(
                        result,
                        control.subMenu,
                        out copiedSubMenuPath,
                        isSubMenu: true
                    );

                    resourceControl.subMenuGuid = subMenuResource.Guid;
                }

                copiedResourcePaths[index] = (copiedIconPath, iconName, copiedSubMenuPath);

                expressionsMenuResource.controls.Add(resourceControl);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return expressionsMenuResource;
        }
    }
#endif
}
