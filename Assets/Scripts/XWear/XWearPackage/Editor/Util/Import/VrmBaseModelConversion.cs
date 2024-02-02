using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using XWear.IO;
using XWear.IO.XResource;
using XWear.IO.XResource.Animation;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.AvatarMeta;
using AnimatorControllerLayer = UnityEditor.Animations.AnimatorControllerLayer;

namespace XWear.XWearPackage.Editor.Util.Import
{
    internal static class VrmBaseModelConversion
    {
        private class CopiedFacialResource
        {
            public AnimatorController FaceAnimatorController;
            public AnimatorController HandAnimatorController;
            public VRCExpressionParameters ExpressionParameters;
            public VRCExpressionsMenu ExpressionMenu;
        }

        private struct AnimatorControllerStrings
        {
            public const string LeftHand = "Left Hand";
            public const string RightHand = "Right Hand";
            public const string FixFace = "Fix Face";
            public const string FaceDefault = "FaceDefault";
            public const string TrackingControl = "TrackingControl";
            public const string BlinkDefault = "BlinkDefault";
            public const string ZeroFacial = "ZeroFacial";

            public const string FaceAnimatorController =
                "Source_VRC_Face_AnimatorController.controller";

            public const string HandAnimatorController =
                "Source_VRC_Hand_AnimatorController.controller";

            public const string ExpressionParameters = "Source_VRC_ExpressionParameters.asset";
            public const string ExpressionsMenu = "Source_VRC_ExpressionsMenu.asset";
        }

        private static readonly Dictionary<string, string> FaceAnimationNameToVrcGestureMap =
            new()
            {
                { "angry", "Point" },
                { "happy", "Peace" },
                { "surprised", "RockNRoll" },
                { "sad", "Gun" },
                { "relaxed", "Thumbs up" },
            };

        /// <summary>
        /// VRMを素体として.xavatar出力をおこなった場合、AnimatorControllerの参照と
        /// VrcExpressionの参照が存在しない状態で出力される
        /// UnityEditor内に存在しているPresetからそれぞれのアサインをおこなう
        /// </summary>
        public static void Convert(XItemInstance xItemInstance, AssetSaver assetSaver)
        {
            var instance = xItemInstance.XResourceInstances[0].Instance;
            var avatarDescriptor = instance.GetComponent<VRCAvatarDescriptor>();
            ConvertFacialAnimations(
                assetSaver,
                out var copiedFacialResource,
                out var fixedExpressions
            );

            avatarDescriptor.customizeAnimationLayers = true;
            SetBaseAnimLayers(avatarDescriptor, copiedFacialResource);
            SetSpecialLayers(avatarDescriptor);

            avatarDescriptor.customExpressions = true;
            SetExpressions(avatarDescriptor, copiedFacialResource, fixedExpressions);
        }

        private static void SetBaseAnimLayers(
            VRCAvatarDescriptor avatarDescriptor,
            CopiedFacialResource copiedFacialResource
        )
        {
            avatarDescriptor.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[5];
            avatarDescriptor.baseAnimationLayers[0] = new VRCAvatarDescriptor.CustomAnimLayer()
            {
                type = VRCAvatarDescriptor.AnimLayerType.Base,
                isDefault = true
            };
            avatarDescriptor.baseAnimationLayers[1] = new VRCAvatarDescriptor.CustomAnimLayer()
            {
                type = VRCAvatarDescriptor.AnimLayerType.Additive,
                isDefault = true
            };

            avatarDescriptor.baseAnimationLayers[2] = new VRCAvatarDescriptor.CustomAnimLayer()
            {
                type = VRCAvatarDescriptor.AnimLayerType.Gesture,
                isDefault = true
            };

            avatarDescriptor.baseAnimationLayers[3] = new VRCAvatarDescriptor.CustomAnimLayer()
            {
                type = VRCAvatarDescriptor.AnimLayerType.Action,
                isDefault = true
            };

            avatarDescriptor.baseAnimationLayers[4] = new VRCAvatarDescriptor.CustomAnimLayer
            {
                type = VRCAvatarDescriptor.AnimLayerType.FX,
                animatorController = copiedFacialResource.FaceAnimatorController
            };
        }

        private static void SetSpecialLayers(VRCAvatarDescriptor avatarDescriptor)
        {
            avatarDescriptor.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[3];
            avatarDescriptor.specialAnimationLayers[0] = new VRCAvatarDescriptor.CustomAnimLayer()
            {
                type = VRCAvatarDescriptor.AnimLayerType.Sitting,
                isDefault = true
            };
            avatarDescriptor.specialAnimationLayers[1] = new VRCAvatarDescriptor.CustomAnimLayer()
            {
                type = VRCAvatarDescriptor.AnimLayerType.TPose,
                isDefault = true
            };
            avatarDescriptor.specialAnimationLayers[0] = new VRCAvatarDescriptor.CustomAnimLayer()
            {
                type = VRCAvatarDescriptor.AnimLayerType.IKPose,
                isDefault = true
            };
        }

        private static void SetExpressions(
            VRCAvatarDescriptor avatarDescriptor,
            CopiedFacialResource copiedFacialResource,
            List<(string name, int value)> fixedExpressions
        )
        {
            copiedFacialResource.ExpressionMenu.controls = new List<VRCExpressionsMenu.Control>();
            foreach (var fixedExpression in fixedExpressions)
            {
                var newControl = new VRCExpressionsMenu.Control
                {
                    type = VRCExpressionsMenu.Control.ControlType.Toggle,
                    name = fixedExpression.name,
                    value = fixedExpression.value,
                    parameter = new VRCExpressionsMenu.Control.Parameter() { name = "FixFace" }
                };
                copiedFacialResource.ExpressionMenu.controls.Add(newControl);
            }

            avatarDescriptor.expressionParameters = copiedFacialResource.ExpressionParameters;
            avatarDescriptor.expressionsMenu = copiedFacialResource.ExpressionMenu;

            AssetDatabase.SaveAssets();
        }

        private static void ConvertFacialAnimations(
            AssetSaver assetSaver,
            out CopiedFacialResource copiedFacialResource,
            out List<(string name, int value)> fixedExpressions
        )
        {
            var clips = assetSaver.GuidToAnimationClipAssetMap.Select(x => x.Value).ToArray();
            var vrmAnimationPreset = FindAnimationPreset();
            copiedFacialResource = CopyVrcResourceAsset(vrmAnimationPreset, assetSaver);

            var zeroFacialResourceGuid = assetSaver.GuidToXResourceAnimationClip
                .FirstOrDefault(x => x.Value.name == XResourceAnimationClip.ZeroAnimationClipName)
                .Key;
            var zeroFacialClip = assetSaver.GuidToAnimationClipAssetMap[zeroFacialResourceGuid];
            if (zeroFacialClip == null)
            {
                throw new Exception("ZeroFacialClip not found");
            }

            var blinkZeroClip = clips.FirstOrDefault(
                x => x.name == XResourceAnimationClip.BlinkZeroAnimationClipName
            );
            var facialClips = clips
                .Where(x =>
                {
                    if (blinkZeroClip != null)
                    {
                        if (x.name == blinkZeroClip.name)
                        {
                            return false;
                        }
                    }

                    return x.name != zeroFacialClip.name;
                })
                .ToList();

            AssignFacialAnimation(
                copiedFacialResource,
                faceClips: facialClips,
                zeroFacialClip: zeroFacialClip,
                blinkZeroClip: blinkZeroClip,
                out fixedExpressions
            );
        }

        private static VrmAnimatorPreset FindAnimationPreset()
        {
            return Resources.Load<VrmAnimatorPreset>("ForVrmPresets/VrmAnimationPreset");
        }

        /// <summary>
        /// VRChatに必要なAnimatorControllerやScriptableObjectをコピーする
        /// </summary>
        /// <returns></returns>
        private static CopiedFacialResource CopyVrcResourceAsset(
            VrmAnimatorPreset vrmAnimatorPreset,
            AssetSaver assetSaver
        )
        {
            var faceAnimatorControllerTemplate =
                vrmAnimatorPreset.vrcFaceAnimatorControllerTemplate;
            var handAnimatorControllerTemplate =
                vrmAnimatorPreset.vrcHandAnimatorControllerTemplate;

            var expressionParametersBase = vrmAnimatorPreset.vrcExpressionParametersTemplate;
            var expressionsMenuBase = vrmAnimatorPreset.vrcExpressionsMenuTemplate;

            var animatorControllerFolderName = GetAssetFolderPath(
                assetSaver,
                VrcAssetResource.AssetType.AnimatorController
            );

            var expressionFolderName = GetAssetFolderPath(
                assetSaver,
                VrcAssetResource.AssetType.ExpressionParameters
            );

            return new CopiedFacialResource()
            {
                FaceAnimatorController = FileUtil.CopyAsset(
                    faceAnimatorControllerTemplate,
                    animatorControllerFolderName,
                    AnimatorControllerStrings.FaceAnimatorController
                ),
                HandAnimatorController = FileUtil.CopyAsset(
                    handAnimatorControllerTemplate,
                    animatorControllerFolderName,
                    AnimatorControllerStrings.HandAnimatorController
                ),
                ExpressionParameters = FileUtil.CopyAsset(
                    expressionParametersBase,
                    expressionFolderName,
                    AnimatorControllerStrings.ExpressionParameters
                ),
                ExpressionMenu = FileUtil.CopyAsset(
                    expressionsMenuBase,
                    expressionFolderName,
                    AnimatorControllerStrings.ExpressionsMenu
                ),
            };
        }

        private static string GetAssetFolderPath(
            AssetSaver assetSaver,
            VrcAssetResource.AssetType assetType
        )
        {
            var rootFolderRawPath = assetSaver.RootFolderPath;
            var assetRootFolderPath = rootFolderRawPath
                .Replace("\\", "/")
                .Replace(Application.dataPath, "Assets");
            var assetFolderName = assetSaver.GetVrcAssetResourceFolderName(assetType);
            return Path.Combine(assetRootFolderPath, assetFolderName);
        }

        /// <summary>
        /// CopyしたHandAnimatorControllerに対して、AnimationClipの設定を行う
        /// FixFaceに対してだけStateを新規に作成する
        /// </summary>
        /// <param name="copiedFacialResource"></param>
        /// <param name="faceClips"></param>
        /// <param name="zeroFacialClip">FaceDefaultにアサインされるゼロ値のAnimationClip</param>
        /// <param name="blinkZeroClip"></param>
        /// <param name="fixedExpressions"></param>
        private static void AssignFacialAnimation(
            CopiedFacialResource copiedFacialResource,
            List<AnimationClip> faceClips,
            AnimationClip zeroFacialClip,
            AnimationClip blinkZeroClip,
            out List<(string name, int value)> fixedExpressions
        )
        {
            fixedExpressions = new List<(string name, int value)>();

            var handExpressionLayers = copiedFacialResource.FaceAnimatorController.layers
                .Where(
                    x =>
                        x.name == AnimatorControllerStrings.LeftHand
                        || x.name == AnimatorControllerStrings.RightHand
                )
                .ToArray();

            var fixFaceExpressionLayer =
                copiedFacialResource.FaceAnimatorController.layers.FirstOrDefault(
                    x => x.name == AnimatorControllerStrings.FixFace
                );

            foreach (var layer in handExpressionLayers)
            {
                foreach (var faceClip in faceClips)
                {
                    var clipName = faceClip.name;
                    if (
                        FaceAnimationNameToVrcGestureMap.TryGetValue(
                            clipName,
                            out var assignTargetStateName
                        )
                    )
                    {
                        AssignAnimatorState(layer, assignTargetStateName, faceClip);
                    }
                }
            }

            if (fixFaceExpressionLayer != null)
            {
                var states = new List<AnimatorState>();
                foreach (var faceClip in faceClips)
                {
                    var state = fixFaceExpressionLayer.stateMachine.AddState(faceClip.name);
                    state.name = faceClip.name;
                    state.motion = faceClip;
                    states.Add(state);
                }

                for (var index = 0; index < states.Count; index++)
                {
                    var state = states[index];
                    var transition = fixFaceExpressionLayer.stateMachine.AddAnyStateTransition(
                        state
                    );
                    var condition = new AnimatorCondition()
                    {
                        mode = AnimatorConditionMode.Equals,
                        parameter = "FixFace",
                        threshold = 3 + index
                    };
                    transition.conditions = new[] { condition };

                    fixedExpressions.Add((state.name, (int)condition.threshold));
                }
            }

            var assignTargetZeroFacialLayer =
                copiedFacialResource.FaceAnimatorController.layers.FirstOrDefault(
                    x => x.name == AnimatorControllerStrings.FaceDefault
                );
            AssignAnimatorState(
                assignTargetZeroFacialLayer,
                AnimatorControllerStrings.ZeroFacial,
                zeroFacialClip
            );

            if (blinkZeroClip != null)
            {
                var assignTargetTrackingControlLayer =
                    copiedFacialResource.FaceAnimatorController.layers.FirstOrDefault(
                        x => x.name == AnimatorControllerStrings.TrackingControl
                    );
                AssignAnimatorState(
                    assignTargetTrackingControlLayer,
                    AnimatorControllerStrings.BlinkDefault,
                    blinkZeroClip
                );
            }
        }

        /// <summary>
        /// AnimatorのレイヤーからStateを漁って、targetStateNameのstateがあればそこにsourceClipを突っ込む
        /// </summary>
        /// <param name="targetLayer"></param>
        /// <param name="targetStateName"></param>
        /// <param name="sourceClip"></param>
        private static void AssignAnimatorState(
            AnimatorControllerLayer targetLayer,
            string targetStateName,
            AnimationClip sourceClip
        )
        {
            var targetState = targetLayer.stateMachine.states.FirstOrDefault(
                x => x.state.name == targetStateName
            );
            targetState.state.motion = sourceClip;
        }
    }
}
