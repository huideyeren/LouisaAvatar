using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDKBase;
using XWear.IO;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.AvatarMeta;
using XWear.IO.XResource.Component.ComponentPlugin;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Transform;

namespace XWear.XWearPackage.ForVrc.ComponentPlugins
{
    public static class VrcAvatarDescriptorBuildUtil
    {
        public static void SetFromXResource(
            VrcAvatarDescriptorParam source,
            VRCAvatarDescriptor result,
            GameObjectWithTransformBuilder gameObjectBuilder,
            SkinnedMeshRendererDataBuilder skinnedMeshRendererDataBuilder,
            XResourceContainerUtil.XResourceOpener opener,
            AssetSaver assetSaver
        )
        {
            var guidToAssetPathMap = ExtractAssetResources(
                source.vrcAssetResources,
                opener: opener,
                assetSaver: assetSaver
            );

            BuildViewPosition(source.viewPosition, result);
            BuildLipSync(source.lipSync, result, gameObjectBuilder, skinnedMeshRendererDataBuilder);
            BuildEyeLook(source.eyeLook, result, gameObjectBuilder, skinnedMeshRendererDataBuilder);
            BuildLowerBody(source.lowerBody, result);
            BuildColliders(source.collider, result, gameObjectBuilder);

#if UNITY_EDITOR
            var vrcAssetResourceBuilder = new VrcAssetResourceBuilder(
                source.vrcAssetResources,
                guidToAssetPathMap
            );

            var guidToBuiltRuntimeAnimatorController =
                vrcAssetResourceBuilder.BuildAnimatorController(
                    guidToAssetPathMap: guidToAssetPathMap,
                    guidToClipAssetMap: assetSaver.GuidToAnimationClipAssetMap
                );

            result.customizeAnimationLayers = source.animationLayers.customizeAnimationLayers;
            result.baseAnimationLayers = AnimationLayerConvert(
                source.animationLayers.baseAnimationLayers,
                guidToBuiltRuntimeAnimatorController: guidToBuiltRuntimeAnimatorController
            );
            result.specialAnimationLayers = AnimationLayerConvert(
                source.animationLayers.specialAnimationLayers,
                guidToBuiltRuntimeAnimatorController: guidToBuiltRuntimeAnimatorController
            );
            BuildExpressions(vrcAssetResourceBuilder, result);
#endif
        }

        private static void BuildViewPosition(VrcViewPosition source, VRCAvatarDescriptor result)
        {
            result.ViewPosition = source.position;
        }

        private static void BuildLipSync(
            VrcLipSync source,
            VRCAvatarDescriptor result,
            GameObjectWithTransformBuilder gameObjectBuilder,
            SkinnedMeshRendererDataBuilder smrBuilder
        )
        {
            switch (source.lipSyncStyle)
            {
                case VrcLipSync.LipSyncStyle.Default:
                    result.lipSync = VRC_AvatarDescriptor.LipSyncStyle.Default;
                    break;
                case VrcLipSync.LipSyncStyle.JawFlapBone:
                    result.lipSync = VRC_AvatarDescriptor.LipSyncStyle.JawFlapBone;
                    break;
                case VrcLipSync.LipSyncStyle.JawFlapBlendShape:
                    result.lipSync = VRC_AvatarDescriptor.LipSyncStyle.JawFlapBone;
                    break;
                case VrcLipSync.LipSyncStyle.VisemeBlendShape:
                    result.lipSync = VRC_AvatarDescriptor.LipSyncStyle.VisemeBlendShape;
                    break;
                case VrcLipSync.LipSyncStyle.VisemeParameterOnly:
                    result.lipSync = VRC_AvatarDescriptor.LipSyncStyle.VisemeParameterOnly;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (result.lipSync == VRC_AvatarDescriptor.LipSyncStyle.VisemeBlendShape)
            {
                result.VisemeSkinnedMesh = BuildUtilForVrcProject.GetBuiltSmrWithCheckStringEmpty(
                    source.visemeSkinnedMeshRendererGuid,
                    smrBuilder
                );
                var resultVisemeBlendShapes = new string[source.visemeBlendShapes.Length];
                for (int i = 0; i < source.visemeBlendShapes.Length; i++)
                {
                    resultVisemeBlendShapes[i] = source.visemeBlendShapes[i];
                }

                result.VisemeBlendShapes = resultVisemeBlendShapes;
            }
            else if (result.lipSync == VRC_AvatarDescriptor.LipSyncStyle.JawFlapBone)
            {
                result.lipSyncJawBone =
                    BuildUtilForVrcProject.GetBuiltTransformWithCheckStringEmpty(
                        source.jawFlapBoneGuid,
                        gameObjectBuilder
                    );
                result.lipSyncJawOpen = source.jawOpen;
                result.lipSyncJawClosed = source.jawClose;
            }
            else if (result.lipSync == VRC_AvatarDescriptor.LipSyncStyle.VisemeParameterOnly)
            {
                result.VisemeSkinnedMesh = BuildUtilForVrcProject.GetBuiltSmrWithCheckStringEmpty(
                    source.visemeSkinnedMeshRendererGuid,
                    smrBuilder
                );
                result.MouthOpenBlendShapeName = source.mouthOpenBlendShapeName;
            }
        }

        private static void BuildEyeLook(
            VrcEyeLook source,
            VRCAvatarDescriptor result,
            GameObjectWithTransformBuilder gameObjectBuilder,
            SkinnedMeshRendererDataBuilder smrBuilder
        )
        {
            result.enableEyeLook = source.enable;
            if (!result.enableEyeLook)
            {
                return;
            }

            var resultSettings = new VRCAvatarDescriptor.CustomEyeLookSettings();

            resultSettings.eyeMovement =
                new VRCAvatarDescriptor.CustomEyeLookSettings.EyeMovements()
                {
                    confidence = source.eyeMovementsConfidence,
                    excitement = source.eyeMovementsExcitement,
                };

            resultSettings.leftEye = BuildUtilForVrcProject.GetBuiltTransformWithCheckStringEmpty(
                source.leftEyeGuid,
                gameObjectBuilder
            );
            resultSettings.rightEye = BuildUtilForVrcProject.GetBuiltTransformWithCheckStringEmpty(
                source.rightEyeGuid,
                gameObjectBuilder
            );

            resultSettings.eyesLookingStraight = EyeRotationsConvert(source.lookingStraight);
            resultSettings.eyesLookingUp = EyeRotationsConvert(source.lookingUp);
            resultSettings.eyesLookingDown = EyeRotationsConvert(source.lookingDown);
            resultSettings.eyesLookingLeft = EyeRotationsConvert(source.lookingLeft);
            resultSettings.eyesLookingRight = EyeRotationsConvert(source.lookingRight);

            switch (source.eyelidType)
            {
                case VrcEyeLook.EyelidType.None:
                    resultSettings.eyelidType = VRCAvatarDescriptor.EyelidType.None;
                    break;
                case VrcEyeLook.EyelidType.Bones:
                    resultSettings.eyelidType = VRCAvatarDescriptor.EyelidType.Bones;
                    break;
                case VrcEyeLook.EyelidType.BlendShapes:
                    resultSettings.eyelidType = VRCAvatarDescriptor.EyelidType.Blendshapes;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (resultSettings.eyelidType == VRCAvatarDescriptor.EyelidType.Blendshapes)
            {
                resultSettings.eyelidsSkinnedMesh =
                    BuildUtilForVrcProject.GetBuiltSmrWithCheckStringEmpty(
                        source.eyelidSkinnedMeshRendererGuid,
                        smrBuilder
                    );

                var resultBlendShapes = new int[source.eyelidBlendShapes.Length];
                for (int i = 0; i < source.eyelidBlendShapes.Length; i++)
                {
                    resultBlendShapes[i] = source.eyelidBlendShapes[i];
                }

                resultSettings.eyelidsBlendshapes = resultBlendShapes;
            }
            else if (resultSettings.eyelidType == VRCAvatarDescriptor.EyelidType.Bones)
            {
                resultSettings.upperLeftEyelid =
                    BuildUtilForVrcProject.GetBuiltTransformWithCheckStringEmpty(
                        source.upperLeftEyelidGuid,
                        gameObjectBuilder
                    );

                resultSettings.upperRightEyelid =
                    BuildUtilForVrcProject.GetBuiltTransformWithCheckStringEmpty(
                        source.upperRightEyelidGuid,
                        gameObjectBuilder
                    );

                resultSettings.lowerLeftEyelid =
                    BuildUtilForVrcProject.GetBuiltTransformWithCheckStringEmpty(
                        source.lowerLeftEyelidGuid,
                        gameObjectBuilder
                    );

                resultSettings.lowerRightEyelid =
                    BuildUtilForVrcProject.GetBuiltTransformWithCheckStringEmpty(
                        source.lowerRightEyelidGuid,
                        gameObjectBuilder
                    );
            }

            resultSettings.eyelidsDefault = EyelidRotationsConvert(source.eyelidsDefault);
            resultSettings.eyelidsClosed = EyelidRotationsConvert(source.eyelidsClosed);
            resultSettings.eyelidsLookingUp = EyelidRotationsConvert(source.eyelidsLookingUp);
            resultSettings.eyelidsLookingDown = EyelidRotationsConvert(source.eyelidsLookingDown);
            result.customEyeLookSettings = resultSettings;
        }

        private static void BuildLowerBody(VrcLowerBody source, VRCAvatarDescriptor result)
        {
            result.autoFootsteps = source.autoStep;
            result.autoLocomotion = source.autoLocomotion;
        }

        private static void BuildColliders(
            VrcCollider source,
            VRCAvatarDescriptor result,
            GameObjectWithTransformBuilder gameObjectBuilder
        )
        {
            result.collider_head = ColliderConfigConvert(source.head, gameObjectBuilder);
            result.collider_torso = ColliderConfigConvert(source.torso, gameObjectBuilder);

            result.collider_handL = ColliderConfigConvert(source.handL, gameObjectBuilder);
            result.collider_handR = ColliderConfigConvert(source.handR, gameObjectBuilder);

            result.collider_footL = ColliderConfigConvert(source.footL, gameObjectBuilder);
            result.collider_footR = ColliderConfigConvert(source.footR, gameObjectBuilder);

            result.collider_fingerIndexL = ColliderConfigConvert(
                source.fingerIndexL,
                gameObjectBuilder
            );
            result.collider_fingerIndexR = ColliderConfigConvert(
                source.fingerIndexR,
                gameObjectBuilder
            );

            result.collider_fingerMiddleL = ColliderConfigConvert(
                source.fingerMiddleL,
                gameObjectBuilder
            );
            result.collider_fingerMiddleR = ColliderConfigConvert(
                source.fingerMiddleR,
                gameObjectBuilder
            );

            result.collider_fingerRingL = ColliderConfigConvert(
                source.fingerRingL,
                gameObjectBuilder
            );
            result.collider_fingerRingR = ColliderConfigConvert(
                source.fingerRingR,
                gameObjectBuilder
            );

            result.collider_fingerLittleL = ColliderConfigConvert(
                source.fingerLittleL,
                gameObjectBuilder
            );
            result.collider_fingerLittleR = ColliderConfigConvert(
                source.fingerLittleR,
                gameObjectBuilder
            );
        }

        private static VRCAvatarDescriptor.ColliderConfig ColliderConfigConvert(
            VrcCollider.Config source,
            GameObjectWithTransformBuilder gameObjectBuilder
        )
        {
            return new VRCAvatarDescriptor.ColliderConfig()
            {
                height = source.height,
                isMirrored = source.isMirrored,
                position = source.position,
                radius = source.radius,
                rotation = source.rotation,
                state = (VRCAvatarDescriptor.ColliderConfig.State)source.state,
                transform = BuildUtilForVrcProject.GetBuiltTransformWithCheckStringEmpty(
                    source.transformGuid,
                    gameObjectBuilder
                )
            };
        }

        private static VRCAvatarDescriptor.CustomAnimLayer[] AnimationLayerConvert(
            VrcAnimationLayers.VrcPlayableLayers[] sourceLayers,
            Dictionary<string, RuntimeAnimatorController> guidToBuiltRuntimeAnimatorController
        )
        {
            var result = new VRCAvatarDescriptor.CustomAnimLayer[sourceLayers.Length];
            for (int i = 0; i < sourceLayers.Length; i++)
            {
                var source = sourceLayers[i];
                var newLayer = new VRCAvatarDescriptor.CustomAnimLayer()
                {
                    animatorController = null,
                    mask = null,
                    isDefault = source.isDefault,
                    isEnabled = source.isEnabled,
                    type = (VRCAvatarDescriptor.AnimLayerType)source.layerType
                };

                if (
                    !string.IsNullOrEmpty(source.animatorGuid)
                    && guidToBuiltRuntimeAnimatorController.TryGetValue(
                        source.animatorGuid,
                        out var animatorController
                    )
                )
                {
                    newLayer.animatorController = animatorController;
                }

                result[i] = newLayer;
            }

            return result;
        }

        private static VRCAvatarDescriptor.CustomEyeLookSettings.EyeRotations EyeRotationsConvert(
            VrcEyeLook.EyeRotationSet source
        )
        {
            return new VRCAvatarDescriptor.CustomEyeLookSettings.EyeRotations()
            {
                left = source.left,
                right = source.right,
                linked = source.linked
            };
        }

        private static VRCAvatarDescriptor.CustomEyeLookSettings.EyelidRotations EyelidRotationsConvert(
            VrcEyeLook.EyelidRotationSet source
        )
        {
            return new VRCAvatarDescriptor.CustomEyeLookSettings.EyelidRotations()
            {
                lower = EyeRotationsConvert(source.lower),
                upper = EyeRotationsConvert(source.upper)
            };
        }

        private static Dictionary<string, string> ExtractAssetResources(
            List<VrcAssetResource> vrcAssetResources,
            XResourceContainerUtil.XResourceOpener opener,
            AssetSaver assetSaver
        )
        {
            var result = new Dictionary<string, string>();
            if (assetSaver == null)
            {
                return result;
            }

#if UNITY_EDITOR
            foreach (var vrcAssetResource in vrcAssetResources)
            {
                var assetPath = assetSaver.CreateVrcAssets(opener, vrcAssetResource);
                result.Add(vrcAssetResource.Guid, assetPath);
            }

            UnityEditor.AssetDatabase.Refresh();
            UnityEditor.AssetDatabase.SaveAssets();
#endif
            return result;
        }

#if UNITY_EDITOR
        private static void BuildExpressions(
            VrcAssetResourceBuilder assetResourceBuilder,
            VRCAvatarDescriptor result
        )
        {
            assetResourceBuilder.BuildVrcExpressionsMenus(out var mainMenu);
            var parameters = assetResourceBuilder.BuildVrcExpressionsParameter();

            if (mainMenu != null || parameters != null)
            {
                result.customExpressions = true;
            }

            result.expressionsMenu = mainMenu;
            result.expressionParameters = parameters;
        }
#endif
    }

    public static class BuildUtilForVrcProject
    {
        public static UnityEngine.Transform GetBuiltTransformWithCheckStringEmpty(
            string guid,
            GameObjectWithTransformBuilder gameObjectBuilder
        )
        {
            if (!string.IsNullOrEmpty(guid))
            {
                var result = BuildComponentUtil.GetBuildTransformRefFromGuid(
                    gameObjectBuilder,
                    guid
                );
                return result;
            }

            return null;
        }

        public static UnityEngine.SkinnedMeshRenderer GetBuiltSmrWithCheckStringEmpty(
            string guid,
            SkinnedMeshRendererDataBuilder smrBuilder
        )
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            return smrBuilder.GuidToBuiltSkinnedMeshRenderer.TryGetValue(guid, out var result)
                ? result
                : null;
        }
    }
}
