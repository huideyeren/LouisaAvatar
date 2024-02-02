using System.Collections.Generic;
using UnityEngine;

namespace XWear.XWearPackage.Editor.Util.ExtraDrawer
{
    public static class ZoomIconGuiInfo
    {
        public static readonly Dictionary<
            BodyPartGroupDrawer.PartGroupType,
            List<ZoomIconContent>
        > ZoomIconContents =
            new()
            {
                {
                    #region LeftHand

                    BodyPartGroupDrawer.PartGroupType.LeftHand,
                    new List<ZoomIconContent>()
                    {
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.LeftHand,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        // LeftIndex
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.LeftIndexProximal,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.LeftIndexIntermediate,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.LeftIndexDistal,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        // LeftMiddle
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.LeftMiddleProximal,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.LeftMiddleIntermediate,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.LeftMiddleDistal,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        // LeftRing
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.LeftRingProximal,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.LeftRingIntermediate,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.LeftRingDistal,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        // LeftLittle
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.LeftLittleProximal,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.LeftLittleIntermediate,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.LeftLittleDistal,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        // LeftThumb
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.LeftThumbProximal,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.LeftThumbIntermediate,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.LeftThumbDistal,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        }
                    }

                    #endregion
                },
                {
                    #region RightHand

                    BodyPartGroupDrawer.PartGroupType.RightHand,
                    new List<ZoomIconContent>()
                    {
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.RightHand,
                            ButtonOffsetRatio = new Vector2(0.3f, -0.1f)
                        },
                        // RightIndex
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.RightIndexProximal,
                            ButtonOffsetRatio = new Vector2(-0.1f, 0.2f)
                        },
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.RightIndexIntermediate,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.RightIndexDistal,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        // RightMiddle
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.RightMiddleProximal,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.RightMiddleIntermediate,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.RightMiddleDistal,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        // RightRing
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.RightRingProximal,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.RightRingIntermediate,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.RightRingDistal,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        // RightLittle
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.RightLittleProximal,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.RightLittleIntermediate,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.RightLittleDistal,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        // RightThumb
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.RightThumbProximal,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.RightThumbIntermediate,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        },
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.RightThumbDistal,
                            ButtonOffsetRatio = new Vector2(0, 0)
                        }
                    }

                    #endregion
                },
                {
                    #region Head

                    BodyPartGroupDrawer.PartGroupType.Head,
                    new List<ZoomIconContent>()
                    {
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.Neck,
                            ButtonOffsetRatio = new Vector2(-0.2f, -0.6f)
                        },
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.Head,
                            ButtonOffsetRatio = new Vector2(-0.05f, -0.30f)
                        },
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.LeftEye,
                            ButtonOffsetRatio = new Vector2(0.8f, 0.19f)
                        },
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.RightEye,
                            ButtonOffsetRatio = new Vector2(0.3f, 0.19f)
                        },
                        new()
                        {
                            HumanBodyBones = HumanBodyBones.Jaw,
                            ButtonOffsetRatio = new Vector2(0.55f, -0.4f)
                        },
                    }

                    #endregion
                }
            };
    }
}
