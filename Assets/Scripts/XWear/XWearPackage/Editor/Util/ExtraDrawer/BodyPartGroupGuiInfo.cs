using System;
using UnityEditor;
using UnityEngine;

namespace XWear.XWearPackage.Editor.Util.ExtraDrawer
{
    [Serializable]
    public static class BodyPartGroupGuiInfo
    {
        [SerializeReference]
        public static readonly BodyPartGroupDrawer[] BodyPartContents =
        {
            new BodyPartGroupDrawer(
                BodyPartGroupDrawer.PartGroupType.Torso,
                new HumanBodyBones[]
                {
                    HumanBodyBones.Hips,
                    HumanBodyBones.Spine,
                    HumanBodyBones.Chest,
                    HumanBodyBones.UpperChest,
                },
                "AvatarInspector/Torso",
                buttonOffsetRatio: new Vector2(0, 0.35f)
            ),
            new BodyPartGroupDrawer(
                BodyPartGroupDrawer.PartGroupType.Head,
                new[] { HumanBodyBones.Head, HumanBodyBones.Neck },
                "AvatarInspector/Head",
                buttonOffsetRatio: new Vector2(0, 0.8f)
            ),
            new BodyPartGroupDrawer(
                BodyPartGroupDrawer.PartGroupType.LeftArm,
                new[]
                {
                    HumanBodyBones.LeftShoulder,
                    HumanBodyBones.LeftUpperArm,
                    HumanBodyBones.LeftLowerArm
                },
                "AvatarInspector/LeftArm",
                buttonOffsetRatio: new Vector2(0.45f, 0.4f)
            ),
            new BodyPartGroupDrawer(
                BodyPartGroupDrawer.PartGroupType.LeftHand,
                new[]
                {
                    HumanBodyBones.LeftHand,
                    HumanBodyBones.LeftIndexProximal,
                    HumanBodyBones.LeftIndexIntermediate,
                    HumanBodyBones.LeftIndexDistal,
                    HumanBodyBones.LeftMiddleProximal,
                    HumanBodyBones.LeftMiddleIntermediate,
                    HumanBodyBones.LeftMiddleDistal,
                    HumanBodyBones.LeftRingProximal,
                    HumanBodyBones.LeftRingIntermediate,
                    HumanBodyBones.LeftRingDistal,
                    HumanBodyBones.LeftLittleProximal,
                    HumanBodyBones.LeftLittleIntermediate,
                    HumanBodyBones.LeftLittleDistal,
                    HumanBodyBones.LeftThumbProximal,
                    HumanBodyBones.LeftThumbIntermediate,
                    HumanBodyBones.LeftThumbDistal,
                },
                "AvatarInspector/LeftFingers",
                buttonOffsetRatio: new Vector2(0.7f, 0.0f)
            ),
            new BodyPartGroupDrawer(
                BodyPartGroupDrawer.PartGroupType.RightArm,
                new[]
                {
                    HumanBodyBones.RightShoulder,
                    HumanBodyBones.RightUpperArm,
                    HumanBodyBones.RightLowerArm
                },
                "AvatarInspector/RightArm",
                buttonOffsetRatio: new Vector2(-0.45f, 0.4f)
            ),
            new BodyPartGroupDrawer(
                BodyPartGroupDrawer.PartGroupType.RightHand,
                new[]
                {
                    HumanBodyBones.RightHand,
                    HumanBodyBones.RightIndexProximal,
                    HumanBodyBones.RightIndexIntermediate,
                    HumanBodyBones.RightIndexDistal,
                    HumanBodyBones.RightMiddleProximal,
                    HumanBodyBones.RightMiddleIntermediate,
                    HumanBodyBones.RightMiddleDistal,
                    HumanBodyBones.RightRingProximal,
                    HumanBodyBones.RightRingIntermediate,
                    HumanBodyBones.RightRingDistal,
                    HumanBodyBones.RightLittleProximal,
                    HumanBodyBones.RightLittleIntermediate,
                    HumanBodyBones.RightLittleDistal,
                    HumanBodyBones.RightThumbProximal,
                    HumanBodyBones.RightThumbIntermediate,
                    HumanBodyBones.RightThumbDistal,
                },
                "AvatarInspector/RightFingers",
                buttonOffsetRatio: new Vector2(-0.7f, 0.0f)
            ),
            new BodyPartGroupDrawer(
                BodyPartGroupDrawer.PartGroupType.LeftLeg,
                new[]
                {
                    HumanBodyBones.LeftUpperLeg,
                    HumanBodyBones.LeftLowerLeg,
                    HumanBodyBones.LeftFoot,
                    HumanBodyBones.LeftToes
                },
                "AvatarInspector/LeftLeg",
                buttonOffsetRatio: new Vector2(0.22f, -0.4f)
            ),
            new BodyPartGroupDrawer(
                BodyPartGroupDrawer.PartGroupType.RightLeg,
                new[]
                {
                    HumanBodyBones.RightUpperLeg,
                    HumanBodyBones.RightLowerLeg,
                    HumanBodyBones.RightFoot,
                    HumanBodyBones.RightToes
                },
                "AvatarInspector/RightLeg",
                buttonOffsetRatio: new Vector2(-0.22f, -0.4f)
            ),
        };

        public static readonly GUIContent BodySilhouetteContent = EditorGUIUtility.IconContent(
            "AvatarInspector/BodySilhouette"
        );
    }
}
