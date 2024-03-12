using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XWear.IO;

namespace XWear.XWearPackage.Editor.Util.Export
{
    public static class RemoveBonesUtil
    {
        public static void RemoveUnusedBones(
            HumanoidMapComponent humanoidMapComponent,
            SkinnedMeshRenderer[] skinnedMeshRenderers
        )
        {
            RemoveNullIndexes(skinnedMeshRenderers);
            RemoveZeroWeightedBones(humanoidMapComponent, skinnedMeshRenderers);
        }

        private static void RemoveNullIndexes(SkinnedMeshRenderer[] skinnedMeshRenderers)
        {
            foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
            {
                var invalidIndexes = skinnedMeshRenderer.bones
                    .Select((transform, index) => (transform, index))
                    .Where(x => x.transform == null)
                    .Select(x => x.index)
                    .ToList();

                var validBones = ExtractValidBones(skinnedMeshRenderer, invalidIndexes);
                var validBoneWeights = ExtractValidBoneWeights(skinnedMeshRenderer, invalidIndexes);

                skinnedMeshRenderer.bones = validBones.validBones;
                var sharedMesh = skinnedMeshRenderer.sharedMesh;
                sharedMesh.bindposes = validBones.validBindPoses;
                sharedMesh.boneWeights = validBoneWeights;
            }
        }

        /// <summary>
        /// SkinnedMeshRenderer.bonesの内、いずれからもウェイトが塗られていないボーンを抜く
        /// この関数は与えたSkinnedMeshRendererのsharedMeshやbonesを直接操作する
        /// </summary>
        private static void RemoveZeroWeightedBones(
            HumanoidMapComponent humanoidMapComponent,
            SkinnedMeshRenderer[] skinnedMeshRenderers
        )
        {
            // 各SkinnedMeshRendererからウェイトが塗られているTransformと、ゼロウェイトなTransformの一覧を収集し、キャッシュ
            var weightInfoMap =
                new Dictionary<
                    SkinnedMeshRenderer,
                    (List<Transform> weightedBones, List<Transform> zeroWeightedBones)
                >();

            foreach (var smr in skinnedMeshRenderers)
            {
                weightInfoMap.Add(smr, MeshUtil.GetWeightInfo(smr, smr.sharedMesh));
            }

            var humanoidBoneTransforms = humanoidMapComponent.HumanoidMap.humanoidBones
                .Select(x => x.bone)
                .ToArray();
            var hipsTransform = humanoidMapComponent.HumanoidMap.humanoidBones
                .FirstOrDefault(x => x.humanBodyBones == HumanBodyBones.Hips)
                ?.bone;
            var rootBones = skinnedMeshRenderers.Select(x => x.rootBone).ToArray();
            var removableTransforms = ExtractRemovableZeroWeightedBones(
                hipsTransform: hipsTransform,
                rootBones: rootBones,
                humanoidBoneTransforms: humanoidBoneTransforms,
                weightInfoMap: weightInfoMap
            );

            foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
            {
                var removableIndexes = ExtractRemovableBoneIndexes(
                    skinnedMeshRenderer,
                    removableTransforms
                );
                var validBones = ExtractValidBones(skinnedMeshRenderer, removableIndexes);
                var validBoneWeights = ExtractValidBoneWeights(
                    skinnedMeshRenderer,
                    removableIndexes
                );

                skinnedMeshRenderer.bones = validBones.validBones;
                var sharedMesh = skinnedMeshRenderer.sharedMesh;
                sharedMesh.bindposes = validBones.validBindPoses;
                sharedMesh.boneWeights = validBoneWeights;
            }
        }

        /// <summary>
        /// 出力対象のオブジェクトが持つ全てのSkinnedMeshRenderer上でウェイト値がゼロであるボーンを抽出する
        /// </summary>
        /// <param name="hipsTransform"></param>
        /// <param name="rootBones"></param>
        /// <param name="humanoidBoneTransforms"></param>
        /// <param name="weightInfoMap"></param>
        /// <returns></returns>
        private static List<Transform> ExtractRemovableZeroWeightedBones(
            Transform hipsTransform,
            Transform[] rootBones,
            Transform[] humanoidBoneTransforms,
            Dictionary<
                SkinnedMeshRenderer,
                (List<Transform> weightedBones, List<Transform> zeroWeightedBones)
            > weightInfoMap
        )
        {
            var result = new List<Transform>();

            var allWeightedBones = weightInfoMap
                .SelectMany(x => x.Value.weightedBones)
                .Distinct()
                .ToList();
            var allZeroWeightedBone = weightInfoMap
                .SelectMany(x => x.Value.zeroWeightedBones)
                .Distinct()
                .ToList();

            foreach (var zeroWeightedBone in allZeroWeightedBone)
            {
                if (allWeightedBones.Contains(zeroWeightedBone))
                {
                    continue;
                }

                // HumanoidBoneは無視
                if (humanoidBoneTransforms.Contains(zeroWeightedBone))
                {
                    continue;
                }

                // Rootになっている親は抜きたくないので親がHipsの親と同じであれば無視
                if (hipsTransform != null)
                {
                    if (ReferenceEquals(hipsTransform.parent, zeroWeightedBone))
                    {
                        continue;
                    }
                }

                // SkinnedMeshRendererのRootBoneは抜きたくないので無視
                if (rootBones.Contains(zeroWeightedBone))
                {
                    continue;
                }

                // 親にウェイトが存在するとき、エンドボーンの可能性があるので無視
                if (allWeightedBones.Contains(zeroWeightedBone.parent))
                {
                    continue;
                }

                // 子に非Humanoidなウェイトが塗られたボーンがある場合、揺れもの用のRootボーンの可能性があるので無視
                var childCount = zeroWeightedBone.childCount;
                var containsWeightedChildNotHumanoid = false;
                for (int childIndex = 0; childIndex < childCount; childIndex++)
                {
                    var childBone = zeroWeightedBone.GetChild(childIndex);
                    if (humanoidBoneTransforms.Contains(childBone))
                    {
                        continue;
                    }

                    if (allWeightedBones.Contains(childBone))
                    {
                        containsWeightedChildNotHumanoid = true;
                        break;
                    }
                }

                if (containsWeightedChildNotHumanoid)
                {
                    continue;
                }

                // 非アクティブであればいずれにせよ収集対象ではない
                // ウェイトが塗られているにも関わらず非アクティブになっている場合は
                // バリデータによって、エラーとして返却されている
                if (!zeroWeightedBone.gameObject.activeInHierarchy)
                {
                    continue;
                }

                result.Add(zeroWeightedBone);
            }

            return result;
        }

        /// <summary>
        /// 抜いても問題ないボーンのインデックスを取得する
        /// </summary>
        /// <param name="skinnedMeshRenderer"></param>
        /// <param name="removableTransforms"></param>
        /// <returns></returns>
        private static List<int> ExtractRemovableBoneIndexes(
            SkinnedMeshRenderer skinnedMeshRenderer,
            List<Transform> removableTransforms
        )
        {
            var result = new List<int>();
            var bones = skinnedMeshRenderer.bones;
            for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
            {
                var bone = bones[boneIndex];
                if (removableTransforms.Contains(bone))
                {
                    result.Add(boneIndex);
                }
            }

            return result;
        }

        /// <summary>
        /// 抜いても問題ないボーンをSkinnedMeshRenderer.bones、およびBindPoseから抜く
        /// </summary>
        /// <param name="skinnedMeshRenderer"></param>
        /// <param name="removableBoneIndexes"></param>
        private static (Transform[] validBones, Matrix4x4[] validBindPoses) ExtractValidBones(
            SkinnedMeshRenderer skinnedMeshRenderer,
            List<int> removableBoneIndexes
        )
        {
            var bones = skinnedMeshRenderer.bones;
            var bindPoses = skinnedMeshRenderer.sharedMesh.bindposes;
            var resultBones = bones.ToList();
            var resultBindPoses = bindPoses.Select(bindPose => (Matrix4x4?)bindPose).ToList();

            for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
            {
                if (removableBoneIndexes.Contains(boneIndex))
                {
                    resultBones[boneIndex] = null;
                    resultBindPoses[boneIndex] = null;
                }
            }

            return (
                resultBones.Where(x => x != null).ToArray(),
                resultBindPoses.Where(x => x != null).Select(x => x.Value).ToArray()
            );
        }

        /// <summary>
        /// 抜いた分、BoneWeightの帳尻を合わせる
        /// </summary>
        /// <param name="skinnedMeshRenderer"></param>
        /// <param name="removableBoneIndexes"></param>
        private static BoneWeight[] ExtractValidBoneWeights(
            SkinnedMeshRenderer skinnedMeshRenderer,
            List<int> removableBoneIndexes
        )
        {
            var resultBoneWeights = skinnedMeshRenderer.sharedMesh.boneWeights.ToArray();
            var descendingInvalidBoneIndexes = removableBoneIndexes
                .OrderByDescending(x => x)
                .ToArray();
            for (int vertIndex = 0; vertIndex < resultBoneWeights.Length; vertIndex++)
            {
                var updatedIndex0 = CalculateUpdatedBoneIndex(
                    resultBoneWeights[vertIndex].boneIndex0,
                    descendingInvalidBoneIndexes
                );
                var updatedIndex1 = CalculateUpdatedBoneIndex(
                    resultBoneWeights[vertIndex].boneIndex1,
                    descendingInvalidBoneIndexes
                );
                var updatedIndex2 = CalculateUpdatedBoneIndex(
                    resultBoneWeights[vertIndex].boneIndex2,
                    descendingInvalidBoneIndexes
                );
                var updatedIndex3 = CalculateUpdatedBoneIndex(
                    resultBoneWeights[vertIndex].boneIndex3,
                    descendingInvalidBoneIndexes
                );
                resultBoneWeights[vertIndex].boneIndex0 = updatedIndex0;
                resultBoneWeights[vertIndex].boneIndex1 = updatedIndex1;
                resultBoneWeights[vertIndex].boneIndex2 = updatedIndex2;
                resultBoneWeights[vertIndex].boneIndex3 = updatedIndex3;
            }

            return resultBoneWeights;

            int CalculateUpdatedBoneIndex(int originalIndex, int[] removeIndexes)
            {
                int updatedIndex = originalIndex;

                foreach (int removedIndex in removeIndexes)
                {
                    if (originalIndex > removedIndex)
                    {
                        updatedIndex--;
                    }
                }

                return updatedIndex;
            }
        }
    }
}
