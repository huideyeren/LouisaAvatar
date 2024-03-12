using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace XWear.IO.XResource.Util
{
    public static class IOParentBoneMapper
    {
        public static UnityEngine.Transform GetValidHumanoidParent(
            HumanoidMap humanoidMap,
            HumanBodyBones humanBodyBones
        )
        {
            var result = GetHumanoidParent(humanoidMap, humanBodyBones);
            if (result == null)
            {
                if (
                    HumanBodyBonesParentMap.TryGetValue(
                        humanBodyBones,
                        out var parentHumanBodyBones
                    )
                )
                {
                    return GetValidHumanoidParent(humanoidMap, parentHumanBodyBones);
                }
            }
            else
            {
                return result;
            }

            throw new InvalidDataException("Invalid HumanBodyBones?");
        }

        private static UnityEngine.Transform GetBoneTransform(
            this HumanoidMap humanoidMap,
            HumanBodyBones humanBodyBones
        )
        {
            var map = humanoidMap.GetMap.FlipKvp();
            return map.TryGetValue(humanBodyBones, out var transform) ? transform : null;
        }

        private static UnityEngine.Transform GetHumanoidParent(
            HumanoidMap humanoidMap,
            HumanBodyBones humanBodyBones
        )
        {
            switch (humanBodyBones)
            {
                case HumanBodyBones.Hips:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.Hips)?.parent;

                case HumanBodyBones.LeftUpperLeg:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.Hips);

                case HumanBodyBones.RightUpperLeg:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.Hips);

                case HumanBodyBones.LeftLowerLeg:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.LeftUpperLeg);

                case HumanBodyBones.RightLowerLeg:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.RightUpperLeg);

                case HumanBodyBones.LeftFoot:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.LeftLowerLeg);

                case HumanBodyBones.RightFoot:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.RightLowerLeg);

                case HumanBodyBones.Spine:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.Hips);

                case HumanBodyBones.Chest:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.UpperChest);

                case HumanBodyBones.UpperChest:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.Chest);

                case HumanBodyBones.Neck:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.UpperChest);

                case HumanBodyBones.Head:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.Head);

                case HumanBodyBones.LeftShoulder:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.UpperChest);

                case HumanBodyBones.RightShoulder:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.UpperChest);

                case HumanBodyBones.LeftUpperArm:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.LeftShoulder);

                case HumanBodyBones.RightUpperArm:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.RightShoulder);

                case HumanBodyBones.LeftLowerArm:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.LeftUpperArm);

                case HumanBodyBones.RightLowerArm:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.RightUpperArm);

                case HumanBodyBones.LeftHand:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.LeftLowerArm);

                case HumanBodyBones.RightHand:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.RightLowerArm);

                case HumanBodyBones.LeftToes:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.LeftFoot);

                case HumanBodyBones.RightToes:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.RightFoot);

                case HumanBodyBones.LeftEye:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.Head);

                case HumanBodyBones.RightEye:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.Head);

                case HumanBodyBones.Jaw:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.Head);

                case HumanBodyBones.LeftThumbProximal:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.LeftHand);

                case HumanBodyBones.LeftThumbIntermediate:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.LeftThumbProximal);

                case HumanBodyBones.LeftThumbDistal:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);

                case HumanBodyBones.LeftIndexProximal:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.LeftHand);

                case HumanBodyBones.LeftIndexIntermediate:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.LeftIndexProximal);

                case HumanBodyBones.LeftIndexDistal:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate);

                case HumanBodyBones.LeftMiddleProximal:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.LeftHand);

                case HumanBodyBones.LeftMiddleIntermediate:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);

                case HumanBodyBones.LeftMiddleDistal:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate);

                case HumanBodyBones.LeftRingProximal:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.LeftHand);

                case HumanBodyBones.LeftRingIntermediate:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.LeftRingProximal);

                case HumanBodyBones.LeftRingDistal:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.LeftRingIntermediate);

                case HumanBodyBones.LeftLittleProximal:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.LeftHand);

                case HumanBodyBones.LeftLittleIntermediate:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.LeftLittleProximal);

                case HumanBodyBones.LeftLittleDistal:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate);

                case HumanBodyBones.RightThumbProximal:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.RightHand);

                case HumanBodyBones.RightThumbIntermediate:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.RightThumbProximal);

                case HumanBodyBones.RightThumbDistal:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);

                case HumanBodyBones.RightIndexProximal:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.RightHand);

                case HumanBodyBones.RightIndexIntermediate:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.RightIndexProximal);

                case HumanBodyBones.RightIndexDistal:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.RightIndexIntermediate);

                case HumanBodyBones.RightMiddleProximal:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.RightHand);

                case HumanBodyBones.RightMiddleIntermediate:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.RightMiddleProximal);

                case HumanBodyBones.RightMiddleDistal:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate);

                case HumanBodyBones.RightRingProximal:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.RightHand);

                case HumanBodyBones.RightRingIntermediate:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.RightRingProximal);

                case HumanBodyBones.RightRingDistal:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.RightRingIntermediate);

                case HumanBodyBones.RightLittleProximal:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.RightHand);

                case HumanBodyBones.RightLittleIntermediate:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.RightLittleProximal);

                case HumanBodyBones.RightLittleDistal:
                    return humanoidMap.GetBoneTransform(HumanBodyBones.RightLittleIntermediate);
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(humanBodyBones),
                        humanBodyBones,
                        null
                    );
            }
        }

        private static readonly Dictionary<HumanBodyBones, HumanBodyBones> HumanBodyBonesParentMap =
            new Dictionary<HumanBodyBones, HumanBodyBones>()
            {
                { HumanBodyBones.LeftUpperLeg, HumanBodyBones.Hips },
                { HumanBodyBones.RightUpperLeg, HumanBodyBones.Hips },
                { HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftUpperLeg },
                { HumanBodyBones.RightLowerLeg, HumanBodyBones.RightUpperLeg },
                { HumanBodyBones.LeftFoot, HumanBodyBones.LeftLowerLeg },
                { HumanBodyBones.RightFoot, HumanBodyBones.RightLowerLeg },
                { HumanBodyBones.Spine, HumanBodyBones.Hips },
                { HumanBodyBones.Chest, HumanBodyBones.UpperChest },
                { HumanBodyBones.UpperChest, HumanBodyBones.Chest },
                { HumanBodyBones.Neck, HumanBodyBones.UpperChest },
                { HumanBodyBones.Head, HumanBodyBones.Head },
                { HumanBodyBones.LeftShoulder, HumanBodyBones.UpperChest },
                { HumanBodyBones.RightShoulder, HumanBodyBones.UpperChest },
                { HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftShoulder },
                { HumanBodyBones.RightUpperArm, HumanBodyBones.RightShoulder },
                { HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftUpperArm },
                { HumanBodyBones.RightLowerArm, HumanBodyBones.RightUpperArm },
                { HumanBodyBones.LeftHand, HumanBodyBones.LeftLowerArm },
                { HumanBodyBones.RightHand, HumanBodyBones.RightLowerArm },
                { HumanBodyBones.LeftToes, HumanBodyBones.LeftFoot },
                { HumanBodyBones.RightToes, HumanBodyBones.RightFoot },
                { HumanBodyBones.LeftEye, HumanBodyBones.Head },
                { HumanBodyBones.RightEye, HumanBodyBones.Head },
                { HumanBodyBones.Jaw, HumanBodyBones.Head },
                { HumanBodyBones.LeftThumbProximal, HumanBodyBones.LeftHand },
                { HumanBodyBones.LeftThumbIntermediate, HumanBodyBones.LeftThumbProximal },
                { HumanBodyBones.LeftThumbDistal, HumanBodyBones.LeftThumbIntermediate },
                { HumanBodyBones.LeftIndexProximal, HumanBodyBones.LeftHand },
                { HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.LeftIndexProximal },
                { HumanBodyBones.LeftIndexDistal, HumanBodyBones.LeftIndexIntermediate },
                { HumanBodyBones.LeftMiddleProximal, HumanBodyBones.LeftHand },
                { HumanBodyBones.LeftMiddleIntermediate, HumanBodyBones.LeftMiddleProximal },
                { HumanBodyBones.LeftMiddleDistal, HumanBodyBones.LeftMiddleIntermediate },
                { HumanBodyBones.LeftRingProximal, HumanBodyBones.LeftHand },
                { HumanBodyBones.LeftRingIntermediate, HumanBodyBones.LeftRingProximal },
                { HumanBodyBones.LeftRingDistal, HumanBodyBones.LeftRingIntermediate },
                { HumanBodyBones.LeftLittleProximal, HumanBodyBones.LeftHand },
                { HumanBodyBones.LeftLittleIntermediate, HumanBodyBones.LeftLittleProximal },
                { HumanBodyBones.LeftLittleDistal, HumanBodyBones.LeftLittleIntermediate },
                { HumanBodyBones.RightThumbProximal, HumanBodyBones.RightHand },
                { HumanBodyBones.RightThumbIntermediate, HumanBodyBones.RightThumbProximal },
                { HumanBodyBones.RightThumbDistal, HumanBodyBones.RightThumbIntermediate },
                { HumanBodyBones.RightIndexProximal, HumanBodyBones.RightHand },
                { HumanBodyBones.RightIndexIntermediate, HumanBodyBones.RightIndexProximal },
                { HumanBodyBones.RightIndexDistal, HumanBodyBones.RightIndexIntermediate },
                { HumanBodyBones.RightMiddleProximal, HumanBodyBones.RightHand },
                { HumanBodyBones.RightMiddleIntermediate, HumanBodyBones.RightMiddleProximal },
                { HumanBodyBones.RightMiddleDistal, HumanBodyBones.RightMiddleIntermediate },
                { HumanBodyBones.RightRingProximal, HumanBodyBones.RightHand },
                { HumanBodyBones.RightRingIntermediate, HumanBodyBones.RightRingProximal },
                { HumanBodyBones.RightRingDistal, HumanBodyBones.RightRingIntermediate },
                { HumanBodyBones.RightLittleProximal, HumanBodyBones.RightHand },
                { HumanBodyBones.RightLittleIntermediate, HumanBodyBones.RightLittleProximal },
                { HumanBodyBones.RightLittleDistal, HumanBodyBones.RightLittleIntermediate }
            };
    }
}
