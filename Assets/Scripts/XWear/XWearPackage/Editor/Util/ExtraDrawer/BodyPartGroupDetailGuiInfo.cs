using System;
using UnityEditor;
using UnityEngine;

namespace XWear.XWearPackage.Editor.Util.ExtraDrawer
{
    public static class BodyPartGroupDetailGuiInfo
    {
        private static readonly GUIContent HeadDetailContent = EditorGUIUtility.IconContent(
            "AvatarInspector/HeadZoom"
        );

        private static readonly GUIContent LeftHandDetailContent = EditorGUIUtility.IconContent(
            "AvatarInspector/LeftHandZoom"
        );

        private static readonly GUIContent RightHandDetailContent = EditorGUIUtility.IconContent(
            "AvatarInspector/RightHandZoom"
        );

        public static GUIContent GetBodyPartGroupDetailIcon(
            BodyPartGroupDrawer.PartGroupType partGroupType
        )
        {
            switch (partGroupType)
            {
                case BodyPartGroupDrawer.PartGroupType.Head:
                    return HeadDetailContent;
                case BodyPartGroupDrawer.PartGroupType.LeftHand:
                    return LeftHandDetailContent;
                case BodyPartGroupDrawer.PartGroupType.RightHand:
                    return RightHandDetailContent;
                default:
                    return null;
            }
        }

        public static float GetIconScaleRatio(BodyPartGroupDrawer.PartGroupType partGroupType)
        {
            switch (partGroupType)
            {
                case BodyPartGroupDrawer.PartGroupType.Head:
                    return 1;
                case BodyPartGroupDrawer.PartGroupType.Torso:
                    return 1.5f;
                case BodyPartGroupDrawer.PartGroupType.LeftArm:
                    return 1.5f;
                case BodyPartGroupDrawer.PartGroupType.RightArm:
                    return 1.5f;
                case BodyPartGroupDrawer.PartGroupType.LeftHand:
                    return 1;
                case BodyPartGroupDrawer.PartGroupType.RightHand:
                    return 1;
                case BodyPartGroupDrawer.PartGroupType.LeftLeg:
                    return 1.5f;
                case BodyPartGroupDrawer.PartGroupType.RightLeg:
                    return 1.5f;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(partGroupType),
                        partGroupType,
                        null
                    );
            }
        }

        public static Vector2 GetIconPositionOffsetRatio(
            BodyPartGroupDrawer.PartGroupType partGroupType
        )
        {
            switch (partGroupType)
            {
                case BodyPartGroupDrawer.PartGroupType.Head:
                    return Vector2.one;
                case BodyPartGroupDrawer.PartGroupType.Torso:
                    return Vector2.one;
                case BodyPartGroupDrawer.PartGroupType.LeftArm:
                    return Vector2.one;
                case BodyPartGroupDrawer.PartGroupType.RightArm:
                    return Vector2.one;
                case BodyPartGroupDrawer.PartGroupType.LeftHand:
                    return Vector2.one;
                case BodyPartGroupDrawer.PartGroupType.RightHand:
                    return Vector2.one;
                case BodyPartGroupDrawer.PartGroupType.LeftLeg:
                    return new Vector2(1, 0f);
                case BodyPartGroupDrawer.PartGroupType.RightLeg:
                    return new Vector2(1, 0f);
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(partGroupType),
                        partGroupType,
                        null
                    );
            }
        }

        private static readonly Vector2[] HeadButtonOffsetRatios = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(0, 0.7f),
            new Vector2(0, 0.5f),
        };

        public static Vector2[] GetButtonOffsetRatios(
            BodyPartGroupDrawer.PartGroupType partGroupType
        )
        {
            switch (partGroupType)
            {
                case BodyPartGroupDrawer.PartGroupType.Head:
                    return HeadButtonOffsetRatios;
                case BodyPartGroupDrawer.PartGroupType.Torso:
                    return HeadButtonOffsetRatios;
                case BodyPartGroupDrawer.PartGroupType.LeftArm:
                    return HeadButtonOffsetRatios;
                case BodyPartGroupDrawer.PartGroupType.RightArm:
                    return HeadButtonOffsetRatios;
                case BodyPartGroupDrawer.PartGroupType.LeftHand:
                    return HeadButtonOffsetRatios;
                case BodyPartGroupDrawer.PartGroupType.RightHand:
                    return HeadButtonOffsetRatios;
                case BodyPartGroupDrawer.PartGroupType.LeftLeg:
                    return HeadButtonOffsetRatios;
                case BodyPartGroupDrawer.PartGroupType.RightLeg:
                    return HeadButtonOffsetRatios;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(partGroupType),
                        partGroupType,
                        null
                    );
            }
        }
    }
}
