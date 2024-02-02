using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace XWear.XWearPackage.Editor.Util
{
    public static class MeshUtil
    {
        /// <summary>
        /// 与えたSkinnedMeshRendererからウェイトが塗られているボーンと、塗られていないボーンをそれぞれ抽出する
        /// </summary>
        /// <param name="smr"></param>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static (
            List<Transform> weightedBoneIndexes,
            List<Transform> zeroWeightedBoneIndexes
        ) GetWeightInfo(SkinnedMeshRenderer smr, Mesh mesh)
        {
            var weightedBones = new List<Transform>();
            var zeroWeightedBones = new List<Transform>();
            if (mesh == null || mesh.boneWeights == null)
            {
                return (weightedBones, zeroWeightedBones);
            }

            var boneWeights = mesh.boneWeights;
            for (var index = 0; index < boneWeights.Length; index++)
            {
                var boneWeight = boneWeights[index];
                if (boneWeight.weight0 != 0.0f)
                {
                    var bone = smr.bones[boneWeight.boneIndex0];
                    if (!weightedBones.Contains(bone))
                    {
                        weightedBones.Add(bone);
                    }
                }

                if (boneWeight.weight1 != 0.0f)
                {
                    var bone = smr.bones[boneWeight.boneIndex1];
                    if (!weightedBones.Contains(bone))
                    {
                        weightedBones.Add(bone);
                    }
                }

                if (boneWeight.weight2 != 0.0f)
                {
                    var bone = smr.bones[boneWeight.boneIndex2];
                    if (!weightedBones.Contains(bone))
                    {
                        weightedBones.Add(bone);
                    }
                }

                if (boneWeight.weight3 != 0.0f)
                {
                    var bone = smr.bones[boneWeight.boneIndex3];
                    if (!weightedBones.Contains(bone))
                    {
                        weightedBones.Add(bone);
                    }
                }
            }

            // weightedBoneIndexesに含まれていないSkinnedMeshRenderer.bonesの中身の一覧
            zeroWeightedBones = smr.bones
                .Where(x => !weightedBones.Contains(x))
                .Select(x => x)
                .ToList();

            return (weightedBones, zeroWeightedBones);
        }
    }
}
