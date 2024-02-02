using System.Collections.Generic;
using UnityEngine;

namespace XWear.IO.XResource.Humanoid
{
    public static class HumanoidHierarchyUtil
    {
        public class HierarchyBone
        {
            public HumanBodyBones HumanBodyBones;
            public bool MustBone = true;
        }

        private static readonly Dictionary<
            HumanBodyBones,
            HierarchyBone[]
        > HumanBodyBonesHierarchyMap = new Dictionary<HumanBodyBones, HierarchyBone[]>()
        {
            {
                HumanBodyBones.Hips,
                new[]
                {
                    new HierarchyBone() { HumanBodyBones = HumanBodyBones.Spine },
                    new HierarchyBone() { HumanBodyBones = HumanBodyBones.LeftUpperLeg },
                    new HierarchyBone() { HumanBodyBones = HumanBodyBones.RightUpperLeg }
                }
            },
            {
                HumanBodyBones.Spine,
                new[] { new HierarchyBone() { HumanBodyBones = HumanBodyBones.Chest }, }
            },
            {
                HumanBodyBones.Chest,
                new[]
                {
                    new HierarchyBone()
                    {
                        HumanBodyBones = HumanBodyBones.UpperChest,
                        MustBone = false
                    },
                }
            },
            {
                HumanBodyBones.UpperChest,
                new[] { new HierarchyBone() { HumanBodyBones = HumanBodyBones.Neck, }, }
            },
            {
                HumanBodyBones.Neck,
                new[] { new HierarchyBone() { HumanBodyBones = HumanBodyBones.Head, }, }
            },
            {
                HumanBodyBones.Head,
                new[]
                {
                    new HierarchyBone() { HumanBodyBones = HumanBodyBones.Jaw, MustBone = false },
                    new HierarchyBone()
                    {
                        HumanBodyBones = HumanBodyBones.LeftEye,
                        MustBone = false
                    },
                    new HierarchyBone()
                    {
                        HumanBodyBones = HumanBodyBones.RightEye,
                        MustBone = false
                    },
                }
            },
        };
    }
}
