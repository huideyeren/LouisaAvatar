using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;
using XWear.IO.XResource.AvatarMeta;

namespace XWear.XWearPackage.ForVrc.ComponentPlugins
{
    public static class VrcAvatarDescriptorCopyUtil
    {
        public static void Copy(VRCAvatarDescriptor source, VRCAvatarDescriptor dest)
        {
            dest.ViewPosition = source.ViewPosition;

            dest.lipSync = source.lipSync;
            if (dest.lipSync == VRC_AvatarDescriptor.LipSyncStyle.VisemeBlendShape)
            {
                dest.VisemeSkinnedMesh = source.VisemeSkinnedMesh;
                var destVisemeBlendShapes = new string[source.VisemeBlendShapes.Length];
                for (int i = 0; i < source.VisemeBlendShapes.Length; i++)
                {
                    destVisemeBlendShapes[i] = source.VisemeBlendShapes[i];
                }

                dest.VisemeBlendShapes = source.VisemeBlendShapes;
            }
            else if (dest.lipSync == VRC_AvatarDescriptor.LipSyncStyle.JawFlapBone)
            {
                dest.lipSyncJawBone = source.lipSyncJawBone;
                dest.lipSyncJawOpen = source.lipSyncJawOpen;
                dest.lipSyncJawClosed = source.lipSyncJawClosed;
            }
            else if (dest.lipSync == VRC_AvatarDescriptor.LipSyncStyle.VisemeParameterOnly)
            {
                dest.MouthOpenBlendShapeName = source.MouthOpenBlendShapeName;
            }

            dest.enableEyeLook = source.enableEyeLook;

            var sourceEyeLookSettings = source.customEyeLookSettings;
            var destEyeLookSettings = new VRCAvatarDescriptor.CustomEyeLookSettings()
            {
                eyeMovement = new VRCAvatarDescriptor.CustomEyeLookSettings.EyeMovements()
                {
                    confidence = sourceEyeLookSettings.eyeMovement.confidence,
                    excitement = sourceEyeLookSettings.eyeMovement.excitement,
                },
                leftEye = sourceEyeLookSettings.leftEye,
                rightEye = sourceEyeLookSettings.rightEye,
                eyesLookingStraight = CloneEyeRotations(sourceEyeLookSettings.eyesLookingStraight),
                eyesLookingUp = CloneEyeRotations(sourceEyeLookSettings.eyesLookingUp),
                eyesLookingDown = CloneEyeRotations(sourceEyeLookSettings.eyesLookingDown),
                eyesLookingLeft = CloneEyeRotations(sourceEyeLookSettings.eyesLookingStraight),
                eyesLookingRight = CloneEyeRotations(sourceEyeLookSettings.eyesLookingRight),
                eyelidType = sourceEyeLookSettings.eyelidType,
            };

            if (destEyeLookSettings.eyelidType == VRCAvatarDescriptor.EyelidType.Blendshapes)
            {
                destEyeLookSettings.eyelidsSkinnedMesh = sourceEyeLookSettings.eyelidsSkinnedMesh;
                var destEyelidBlendShapes = new int[
                    sourceEyeLookSettings.eyelidsBlendshapes.Length
                ];
                for (int i = 0; i < sourceEyeLookSettings.eyelidsBlendshapes.Length; i++)
                {
                    destEyelidBlendShapes[i] = sourceEyeLookSettings.eyelidsBlendshapes[i];
                }

                destEyeLookSettings.eyelidsBlendshapes = destEyelidBlendShapes;
            }
            else if (destEyeLookSettings.eyelidType == VRCAvatarDescriptor.EyelidType.Bones)
            {
                destEyeLookSettings.upperLeftEyelid = sourceEyeLookSettings.upperLeftEyelid;
                destEyeLookSettings.upperRightEyelid = sourceEyeLookSettings.upperRightEyelid;
                destEyeLookSettings.lowerLeftEyelid = sourceEyeLookSettings.lowerLeftEyelid;
                destEyeLookSettings.lowerRightEyelid = sourceEyeLookSettings.lowerRightEyelid;

                destEyeLookSettings.eyelidsDefault = CloneEyelidRotations(
                    sourceEyeLookSettings.eyelidsDefault
                );
                destEyeLookSettings.eyelidsClosed = CloneEyelidRotations(
                    sourceEyeLookSettings.eyelidsClosed
                );
                destEyeLookSettings.eyelidsLookingUp = CloneEyelidRotations(
                    sourceEyeLookSettings.eyelidsLookingUp
                );
                destEyeLookSettings.eyelidsLookingDown = CloneEyelidRotations(
                    sourceEyeLookSettings.eyelidsLookingDown
                );
            }

            dest.customEyeLookSettings = destEyeLookSettings;

            dest.customizeAnimationLayers = source.customizeAnimationLayers;
            dest.baseAnimationLayers = CloneCustomAnimLayers(source.baseAnimationLayers);
            dest.specialAnimationLayers = CloneCustomAnimLayers(source.specialAnimationLayers);

            dest.autoFootsteps = source.autoFootsteps;
            dest.autoLocomotion = source.autoLocomotion;

            dest.expressionsMenu = source.expressionsMenu;
            dest.expressionParameters = source.expressionParameters;

            dest.collider_head = CloneColliderConfig(source.collider_head);
            dest.collider_torso = CloneColliderConfig(source.collider_torso);

            dest.collider_handL = CloneColliderConfig(source.collider_handL);
            dest.collider_handR = CloneColliderConfig(source.collider_handR);

            dest.collider_footL = CloneColliderConfig(source.collider_footL);
            dest.collider_footR = CloneColliderConfig(source.collider_footR);

            dest.collider_fingerIndexL = CloneColliderConfig(source.collider_fingerIndexL);
            dest.collider_fingerIndexR = CloneColliderConfig(source.collider_fingerIndexR);

            dest.collider_fingerMiddleL = CloneColliderConfig(source.collider_fingerMiddleL);
            dest.collider_fingerMiddleR = CloneColliderConfig(source.collider_fingerMiddleR);

            dest.collider_fingerRingL = CloneColliderConfig(source.collider_fingerRingL);
            dest.collider_fingerRingR = CloneColliderConfig(source.collider_fingerRingR);

            dest.collider_fingerLittleL = CloneColliderConfig(source.collider_fingerLittleL);
            dest.collider_fingerLittleR = CloneColliderConfig(source.collider_fingerLittleR);
        }

        private static VRCAvatarDescriptor.CustomEyeLookSettings.EyeRotations CloneEyeRotations(
            VRCAvatarDescriptor.CustomEyeLookSettings.EyeRotations source
        )
        {
            return new VRCAvatarDescriptor.CustomEyeLookSettings.EyeRotations()
            {
                right = source.right,
                left = source.left,
                linked = source.linked,
            };
        }

        private static VRCAvatarDescriptor.CustomEyeLookSettings.EyelidRotations CloneEyelidRotations(
            VRCAvatarDescriptor.CustomEyeLookSettings.EyelidRotations source
        )
        {
            return new VRCAvatarDescriptor.CustomEyeLookSettings.EyelidRotations()
            {
                lower = CloneEyeRotations(source.lower),
                upper = CloneEyeRotations(source.upper)
            };
        }

        private static VRCAvatarDescriptor.CustomAnimLayer[] CloneCustomAnimLayers(
            VRCAvatarDescriptor.CustomAnimLayer[] sources
        )
        {
            var result = new VRCAvatarDescriptor.CustomAnimLayer[sources.Length];
            for (int i = 0; i < sources.Length; i++)
            {
                var source = sources[i];
                result[i] = new VRCAvatarDescriptor.CustomAnimLayer()
                {
                    isEnabled = source.isEnabled,
                    type = source.type,
                    isDefault = source.isDefault,
                    animatorController = source.animatorController,
                    mask = source.mask
                };
            }

            return result;
        }

        private static VRCAvatarDescriptor.ColliderConfig CloneColliderConfig(
            VRCAvatarDescriptor.ColliderConfig source
        )
        {
            return new VRCAvatarDescriptor.ColliderConfig()
            {
                height = source.height,
                isMirrored = source.isMirrored,
                position = source.position,
                radius = source.radius,
                rotation = source.rotation,
                state = source.state,
                transform = source.transform
            };
        }
    }
}
