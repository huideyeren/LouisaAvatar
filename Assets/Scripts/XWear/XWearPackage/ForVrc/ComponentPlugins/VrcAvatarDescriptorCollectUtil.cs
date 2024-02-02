using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDKBase;
using XWear.IO.XResource.Animation;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.AvatarMeta;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Transform;

namespace XWear.XWearPackage.ForVrc.ComponentPlugins
{
    public static class VrcAvatarDescriptorCollectUtil
    {
        public static XResourceVrcAvatarDescriptorComponent ToXResource(
            VRCAvatarDescriptor descriptor,
            GameObjectWithTransformCollector gameObjectCollector,
            SkinnedMeshRendererDataCollector skinnedMeshRendererCollector,
            XResourceContainerUtil.XResourceArchiver archiver
        )
        {
            var viewPosition = new VrcViewPosition { position = descriptor.ViewPosition };
            var resultVrcAssetResources = new List<VrcAssetResource>();

            var lipSync = new VrcLipSync();
            switch (descriptor.lipSync)
            {
                case VRC_AvatarDescriptor.LipSyncStyle.Default:
                    lipSync.lipSyncStyle = VrcLipSync.LipSyncStyle.Default;
                    break;
                case VRC_AvatarDescriptor.LipSyncStyle.JawFlapBone:
                    lipSync.lipSyncStyle = VrcLipSync.LipSyncStyle.JawFlapBone;
                    break;
                case VRC_AvatarDescriptor.LipSyncStyle.JawFlapBlendShape:
                    lipSync.lipSyncStyle = VrcLipSync.LipSyncStyle.JawFlapBlendShape;
                    break;
                case VRC_AvatarDescriptor.LipSyncStyle.VisemeBlendShape:
                    lipSync.lipSyncStyle = VrcLipSync.LipSyncStyle.VisemeBlendShape;
                    break;
                case VRC_AvatarDescriptor.LipSyncStyle.VisemeParameterOnly:
                    lipSync.lipSyncStyle = VrcLipSync.LipSyncStyle.VisemeParameterOnly;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (descriptor.lipSync == VRC_AvatarDescriptor.LipSyncStyle.VisemeBlendShape)
            {
                lipSync.visemeSkinnedMeshRendererGuid =
                    CollectUtilForVrcProject.GetCollectedSmrGuidWithCheckNull(
                        descriptor.VisemeSkinnedMesh,
                        skinnedMeshRendererCollector
                    );

                lipSync.visemeBlendShapes = descriptor.VisemeBlendShapes.ToArray();
            }
            else if (descriptor.lipSync == VRC_AvatarDescriptor.LipSyncStyle.JawFlapBone)
            {
                lipSync.jawFlapBoneGuid =
                    CollectUtilForVrcProject.GetCollectedTransformGuidWithCheckNull(
                        descriptor.lipSyncJawBone,
                        gameObjectCollector
                    );
                lipSync.jawOpen = descriptor.lipSyncJawOpen;
                lipSync.jawClose = descriptor.lipSyncJawClosed;
            }
            else if (descriptor.lipSync == VRC_AvatarDescriptor.LipSyncStyle.VisemeParameterOnly)
            {
                lipSync.visemeSkinnedMeshRendererGuid =
                    CollectUtilForVrcProject.GetCollectedSmrGuidWithCheckNull(
                        descriptor.VisemeSkinnedMesh,
                        skinnedMeshRendererCollector
                    );

                lipSync.mouthOpenBlendShapeName = descriptor.MouthOpenBlendShapeName;
            }

            var eyeLook = new VrcEyeLook();
            eyeLook.enable = descriptor.enableEyeLook;
            var customEyeLookSettings = descriptor.customEyeLookSettings;
            var eyeMovements = customEyeLookSettings.eyeMovement;
            eyeLook.eyeMovementsConfidence = eyeMovements.confidence;
            eyeLook.eyeMovementsExcitement = eyeMovements.excitement;

            eyeLook.leftEyeGuid = CollectUtilForVrcProject.GetCollectedTransformGuidWithCheckNull(
                customEyeLookSettings.leftEye,
                gameObjectCollector
            );
            eyeLook.rightEyeGuid = CollectUtilForVrcProject.GetCollectedTransformGuidWithCheckNull(
                customEyeLookSettings.rightEye,
                gameObjectCollector
            );

            var lookingStraight = customEyeLookSettings.eyesLookingStraight;
            var lookingUp = customEyeLookSettings.eyesLookingUp;
            var lookingDown = customEyeLookSettings.eyesLookingDown;
            var lookingLeft = customEyeLookSettings.eyesLookingLeft;
            var lookingRight = customEyeLookSettings.eyesLookingRight;

            eyeLook.lookingStraight = EyeRotationConvert(lookingStraight);
            eyeLook.lookingUp = EyeRotationConvert(lookingUp);
            eyeLook.lookingDown = EyeRotationConvert(lookingDown);
            eyeLook.lookingLeft = EyeRotationConvert(lookingLeft);
            eyeLook.lookingRight = EyeRotationConvert(lookingRight);

            switch (customEyeLookSettings.eyelidType)
            {
                case VRCAvatarDescriptor.EyelidType.None:
                    eyeLook.eyelidType = VrcEyeLook.EyelidType.None;
                    break;
                case VRCAvatarDescriptor.EyelidType.Bones:
                    eyeLook.eyelidType = VrcEyeLook.EyelidType.Bones;
                    break;
                case VRCAvatarDescriptor.EyelidType.Blendshapes:
                    eyeLook.eyelidType = VrcEyeLook.EyelidType.BlendShapes;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (customEyeLookSettings.eyelidType == VRCAvatarDescriptor.EyelidType.Blendshapes)
            {
                eyeLook.eyelidSkinnedMeshRendererGuid =
                    CollectUtilForVrcProject.GetCollectedSmrGuidWithCheckNull(
                        customEyeLookSettings.eyelidsSkinnedMesh,
                        skinnedMeshRendererCollector
                    );
                eyeLook.eyelidBlendShapes = customEyeLookSettings.eyelidsBlendshapes;
            }
            else if (customEyeLookSettings.eyelidType == VRCAvatarDescriptor.EyelidType.Bones)
            {
                eyeLook.upperLeftEyelidGuid =
                    CollectUtilForVrcProject.GetCollectedTransformGuidWithCheckNull(
                        customEyeLookSettings.upperLeftEyelid,
                        gameObjectCollector
                    );

                eyeLook.upperRightEyelidGuid =
                    CollectUtilForVrcProject.GetCollectedTransformGuidWithCheckNull(
                        customEyeLookSettings.upperRightEyelid,
                        gameObjectCollector
                    );

                eyeLook.lowerLeftEyelidGuid =
                    CollectUtilForVrcProject.GetCollectedTransformGuidWithCheckNull(
                        customEyeLookSettings.lowerLeftEyelid,
                        gameObjectCollector
                    );

                eyeLook.lowerRightEyelidGuid =
                    CollectUtilForVrcProject.GetCollectedTransformGuidWithCheckNull(
                        customEyeLookSettings.lowerRightEyelid,
                        gameObjectCollector
                    );

                eyeLook.eyelidsDefault = EyelidRotationsConvert(
                    customEyeLookSettings.eyelidsDefault
                );
                eyeLook.eyelidsClosed = EyelidRotationsConvert(customEyeLookSettings.eyelidsClosed);
                eyeLook.eyelidsLookingUp = EyelidRotationsConvert(
                    customEyeLookSettings.eyelidsLookingUp
                );
                eyeLook.eyelidsLookingDown = EyelidRotationsConvert(
                    customEyeLookSettings.eyelidsLookingDown
                );
            }

            var animationLayers = new VrcAnimationLayers();

            var clipInstanceToXResourceAnimationClip =
                new Dictionary<AnimationClip, XResourceAnimationClip>();

            var baseLayers = descriptor.baseAnimationLayers;
            animationLayers.baseAnimationLayers = new VrcAnimationLayers.VrcPlayableLayers[
                baseLayers.Length
            ];

            for (var index = 0; index < baseLayers.Length; index++)
            {
                var baseAnimationLayer = baseLayers[index];
                animationLayers.baseAnimationLayers[index] = PlayableLayersConvert(
                    baseAnimationLayer,
                    archiver,
                    resultVrcAssetResources,
                    clipInstanceToXResourceAnimationClip
                );
            }

            animationLayers.customizeAnimationLayers = descriptor.customizeAnimationLayers;
            var specialLayers = descriptor.specialAnimationLayers;
            animationLayers.specialAnimationLayers = new VrcAnimationLayers.VrcPlayableLayers[
                specialLayers.Length
            ];
            for (var index = 0; index < specialLayers.Length; index++)
            {
                var specialAnimationLayer = specialLayers[index];
                animationLayers.specialAnimationLayers[index] = PlayableLayersConvert(
                    specialAnimationLayer,
                    archiver,
                    resultVrcAssetResources,
                    clipInstanceToXResourceAnimationClip
                );
            }

            archiver.AddXResourceAnimationClips(
                clipInstanceToXResourceAnimationClip.Select(x => x.Value).ToArray()
            );

            var lowerBody = new VrcLowerBody();
            lowerBody.autoStep = descriptor.autoFootsteps;
            lowerBody.autoLocomotion = descriptor.autoLocomotion;

            var expression = new VrcExpressions();

            var collider = new VrcCollider();
            collider.head = ColliderConfigConvert(descriptor.collider_head, gameObjectCollector);
            collider.torso = ColliderConfigConvert(descriptor.collider_torso, gameObjectCollector);

            collider.handL = ColliderConfigConvert(descriptor.collider_handL, gameObjectCollector);
            collider.handR = ColliderConfigConvert(descriptor.collider_handR, gameObjectCollector);

            collider.footL = ColliderConfigConvert(descriptor.collider_footL, gameObjectCollector);
            collider.footR = ColliderConfigConvert(descriptor.collider_footR, gameObjectCollector);

            collider.fingerIndexL = ColliderConfigConvert(
                descriptor.collider_fingerIndexL,
                gameObjectCollector
            );
            collider.fingerIndexR = ColliderConfigConvert(
                descriptor.collider_fingerIndexR,
                gameObjectCollector
            );

            collider.fingerMiddleL = ColliderConfigConvert(
                descriptor.collider_fingerMiddleL,
                gameObjectCollector
            );
            collider.fingerMiddleR = ColliderConfigConvert(
                descriptor.collider_fingerMiddleR,
                gameObjectCollector
            );

            collider.fingerRingL = ColliderConfigConvert(
                descriptor.collider_fingerRingL,
                gameObjectCollector
            );
            collider.fingerRingR = ColliderConfigConvert(
                descriptor.collider_fingerRingR,
                gameObjectCollector
            );

            collider.fingerLittleL = ColliderConfigConvert(
                descriptor.collider_fingerLittleL,
                gameObjectCollector
            );
            collider.fingerLittleR = ColliderConfigConvert(
                descriptor.collider_fingerLittleR,
                gameObjectCollector
            );

            var param = new VrcAvatarDescriptorParam()
            {
                viewPosition = viewPosition,
                lipSync = lipSync,
                eyeLook = eyeLook,
                animationLayers = animationLayers,
                lowerBody = lowerBody,
                expressions = expression,
                collider = collider
            };

            if (descriptor.expressionsMenu != null)
            {
                param.expressions.expressionMenuGuid = ExpressionsMenuConvert(
                    descriptor.expressionsMenu,
                    archiver,
                    resultVrcAssetResources
                );
            }

            if (descriptor.expressionParameters != null)
            {
                param.expressions.expressionParametersGuid = ExpressionParameterConvert(
                    descriptor.expressionParameters,
                    archiver,
                    resultVrcAssetResources
                );
            }

            param.vrcAssetResources = resultVrcAssetResources;

            return new XResourceVrcAvatarDescriptorComponent(param);
        }

        private static VrcEyeLook.EyeRotationSet EyeRotationConvert(
            VRCAvatarDescriptor.CustomEyeLookSettings.EyeRotations eyeRotations
        )
        {
            return new VrcEyeLook.EyeRotationSet()
            {
                linked = eyeRotations.linked,
                left = eyeRotations.left,
                right = eyeRotations.right
            };
        }

        private static VrcEyeLook.EyelidRotationSet EyelidRotationsConvert(
            VRCAvatarDescriptor.CustomEyeLookSettings.EyelidRotations eyelidRotations
        )
        {
            return new VrcEyeLook.EyelidRotationSet()
            {
                lower = EyeRotationConvert(eyelidRotations.lower),
                upper = EyeRotationConvert(eyelidRotations.upper)
            };
        }

        private static VrcAnimationLayers.VrcPlayableLayers PlayableLayersConvert(
            VRCAvatarDescriptor.CustomAnimLayer customAnimLayer,
            XResourceContainerUtil.XResourceArchiver archiver,
            List<VrcAssetResource> resultVrcAssetResources,
            Dictionary<AnimationClip, XResourceAnimationClip> clipInstanceToXResourceAnimationClips
        )
        {
            var result = new VrcAnimationLayers.VrcPlayableLayers()
            {
                isEnabled = customAnimLayer.isEnabled,
                layerType = (int)customAnimLayer.type,
                isDefault = customAnimLayer.isDefault,
            };

#if UNITY_EDITOR
            var collectAssetResourceResults =
                new List<(VrcAssetResource assetResource, string tmpPath)>();

            if (customAnimLayer.animatorController != null)
            {
                var animatorResource =
                    VrcEditorResourceConverter.CopyAnimatorControllerWithAnimation(
                        collectAssetResourceResults,
                        clipInstanceToXResourceAnimationClips,
                        customAnimLayer.animatorController
                    );
                result.animatorGuid = animatorResource.Guid;
            }

            foreach (var collectResult in collectAssetResourceResults)
            {
                archiver.AddVrcAssetResources(collectResult.assetResource, collectResult.tmpPath);
                resultVrcAssetResources.Add(collectResult.assetResource);
            }

#endif

            return result;
        }

        private static string ExpressionsMenuConvert(
            VRCExpressionsMenu expressionsMenu,
            XResourceContainerUtil.XResourceArchiver archiver,
            List<VrcAssetResource> resultVrcAssetResources
        )
        {
            var result = "";

#if UNITY_EDITOR
            var collectAssetResourceResults =
                new List<(VrcAssetResource assetResource, string tmpPath)>();
            var mainMenu = VrcEditorResourceConverter.CopyExpressionsMenu(
                collectAssetResourceResults,
                expressionsMenu,
                out _
            );

            result = mainMenu.Guid;

            foreach (var collectResult in collectAssetResourceResults)
            {
                archiver.AddVrcAssetResources(collectResult.assetResource, collectResult.tmpPath);
                resultVrcAssetResources.Add(collectResult.assetResource);
            }
#endif
            return result;
        }

        private static string ExpressionParameterConvert(
            VRCExpressionParameters expressionParameters,
            XResourceContainerUtil.XResourceArchiver archiver,
            List<VrcAssetResource> resultVrcAssetResources
        )
        {
            var result = "";
#if UNITY_EDITOR

            var copiedPath = IO.Editor.EditorAssetUtil.CopyAssetToTempDir(expressionParameters);
            var expressionsParametersResource = new SimpleVrcAssetResource()
            {
                Guid = Guid.NewGuid().ToString(),
                Name = expressionParameters.name,
                type = VrcAssetResource.AssetType.ExpressionParameters
            };

            var copiedParameter =
                UnityEditor.AssetDatabase.LoadAssetAtPath<VRCExpressionParameters>(copiedPath);
            copiedParameter.name = expressionParameters.name;

            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            archiver.AddVrcAssetResources(expressionsParametersResource, copiedPath);
            result = expressionsParametersResource.Guid;

            resultVrcAssetResources.Add(expressionsParametersResource);
#endif
            return result;
        }

        private static VrcCollider.Config ColliderConfigConvert(
            VRCAvatarDescriptor.ColliderConfig colliderConfig,
            GameObjectWithTransformCollector gameObjectCollector
        )
        {
            return new VrcCollider.Config()
            {
                isMirrored = colliderConfig.isMirrored,
                state = (int)colliderConfig.state,
                transformGuid = CollectUtilForVrcProject.GetCollectedTransformGuidWithCheckNull(
                    colliderConfig.transform,
                    gameObjectCollector
                ),
                radius = colliderConfig.radius,
                height = colliderConfig.height,
                position = colliderConfig.position,
                rotation = colliderConfig.rotation
            };
        }
    }
}
