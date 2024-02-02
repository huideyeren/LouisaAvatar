using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace XWear.IO.XResource.Humanoid
{
    /// <summary>
    /// The following code is a modified version of UniGLTF's code licensed under the MIT License
    /// Copyright (c) 2018 ousttrue
    /// https://github.com/vrm-c/UniVRM/blob/v0.108.0/Assets/UniGLTF/LICENSE.md
    /// original: https://github.com/vrm-c/UniVRM/blob/v0.108.0/Assets/UniGLTF/Runtime/UniHumanoid/HumanoidLoader.cs
    /// </summary>
    public static class HumanoidAvatarUtil
    {
        /// <param name="rootTransform"></param>
        /// <param name="humanoidMap"></param>
        public static UnityEngine.Avatar GenerateFromHumanoidMap(
            UnityEngine.Transform rootTransform,
            HumanoidMap humanoidMap
        )
        {
            var description = new HumanDescription
            {
                skeleton = rootTransform
                    .GetComponentsInChildren<UnityEngine.Transform>()
                    .Select(x => x.ToSkeletonBone())
                    .ToArray(),
                human = humanoidMap.GetMap
                    .Select(x =>
                    {
                        var boneName = x.Key.name;
                        var humanBodyBones = x.Value;
                        return new HumanBone
                        {
                            boneName = boneName,
                            humanName = HumanTraitBoneNameMap[humanBodyBones],
                            limit = new HumanLimit { useDefaultValues = true, }
                        };
                    })
                    .ToArray(),
                armStretch = 0.05f,
                legStretch = 0.05f,
                upperArmTwist = 0.5f,
                lowerArmTwist = 0.5f,
                upperLegTwist = 0.5f,
                lowerLegTwist = 0.5f,
                feetSpacing = 0,
                hasTranslationDoF = false,
            };

            return AvatarBuilder.BuildHumanAvatar(rootTransform.gameObject, description);
        }

        private static SkeletonBone ToSkeletonBone(this UnityEngine.Transform t)
        {
            var sb = new SkeletonBone();
            sb.name = t.name;
            sb.position = t.localPosition;
            sb.rotation = t.localRotation;
            sb.scale = t.localScale;
            return sb;
        }

        private static HumanBodyBones TraitToHumanBone(string x)
        {
            return (HumanBodyBones)Enum.Parse(typeof(HumanBodyBones), x.Replace(" ", ""), true);
        }

        private static readonly Dictionary<HumanBodyBones, string> HumanTraitBoneNameMap =
            HumanTrait.BoneName.ToDictionary(TraitToHumanBone, x => x);
    }
}
