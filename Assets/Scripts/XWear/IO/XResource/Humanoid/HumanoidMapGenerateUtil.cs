using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using XWear.IO.XResource.Humanoid.Estimate;
using XWear.IO.XResource.Util;

namespace XWear.IO.XResource.Humanoid
{
    public static class HumanoidMapGenerateUtil
    {
        public static bool TryGenerate(GameObject rootGameObject, out HumanoidMap result)
        {
            var animator = rootGameObject.GetComponent<Animator>();
            if (animator != null)
            {
                if (TryGenerateFromAnimator(animator, out result))
                {
                    return true;
                }
            }

            if (TryEstimateFromTransformName(rootGameObject, out result))
            {
                return true;
            }

            return false;
        }

        private static bool TryGenerateFromAnimator(Animator animator, out HumanoidMap result)
        {
            result = new HumanoidMap();
            if (!animator.isHuman)
            {
                return false;
            }

            foreach (HumanBodyBones humanBodyBone in Enum.GetValues(typeof(HumanBodyBones)))
            {
                if (humanBodyBone == HumanBodyBones.LastBone)
                {
                    continue;
                }

                var bone = animator.GetBoneTransform(humanBodyBone);
                if (bone == null)
                {
                    continue;
                }

                result.AddHumanoidBone(
                    new HumanoidBone() { humanBodyBones = humanBodyBone, bone = bone }
                );
            }

            return true;
        }

        private static bool TryEstimateFromTransformName(
            GameObject rootGameObject,
            out HumanoidMap result
        )
        {
            result = new HumanoidMap();
            var skinnedMeshRenderers =
                rootGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (skinnedMeshRenderers.Length == 0)
            {
                return false;
            }

            var allBones = skinnedMeshRenderers.SelectMany(x => x.bones).Distinct().ToArray();

            var estimatedBoneMap = Estimate(allBones);

            foreach (var kvp in estimatedBoneMap)
            {
                result.AddHumanoidBone(
                    new HumanoidBone() { humanBodyBones = kvp.Key, bone = kvp.Value }
                );
            }

            return true;
        }

        // todo 今のままでは指の推定が甘い
        private static Dictionary<HumanBodyBones, UnityEngine.Transform> Estimate(
            UnityEngine.Transform[] allBones
        )
        {
            var result = new Dictionary<HumanBodyBones, UnityEngine.Transform>();
            var estimatePatterns = HumanoidEstimateUtil.GenerateEstimatePatterns();
            foreach (var estimatePattern in estimatePatterns)
            {
                foreach (var bone in allBones)
                {
                    if (result.FlipKvp().ContainsKey(bone))
                    {
                        continue;
                    }

                    var boneName = RemoveIgnoreCharacters(bone.gameObject.name);
                    if (estimatePattern.CheckFunction.Invoke(boneName))
                    {
                        if (result.ContainsKey(estimatePattern.HumanBodyBones))
                        {
                            var parentBone = bone.parent;
                            if (parentBone != null)
                            {
                                var parentBoneName = RemoveIgnoreCharacters(
                                    parentBone.gameObject.name
                                );
                                if (estimatePattern.CheckFunction.Invoke(parentBoneName))
                                {
                                    result[estimatePattern.HumanBodyBones] = parentBone;
                                }
                            }
                        }
                        else
                        {
                            result.Add(estimatePattern.HumanBodyBones, bone);
                        }
                    }
                }
            }

            return result;
        }

        private static string RemoveIgnoreCharacters(string input)
        {
            return Regex.Replace(input, pattern: @"[_\.\-\s]", "");
        }
    }
}
