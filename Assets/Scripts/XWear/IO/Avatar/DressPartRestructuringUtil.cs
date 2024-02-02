using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XWear.IO.XResource.Util;

namespace XWear.IO.Avatar
{
    public static class DressPartRestructuringUtil
    {
        /// <summary>
        /// SkinnedMeshRendererを以下の状態に再構築する
        /// - RootBoneの参照が素体側のHipsに差し替わっている
        /// - Humanoidなボーンは素体側の参照に差し替わっている
        /// - 素体側に存在しないボーン、ウェイトが塗られていないボーンがSmr.bones、BindPosesから抜かれている
        /// - BoneWeightの参照するボーンインデックスが有効なボーンだけになるよう詰められている
        /// </summary>
        /// <param name="dressPartHumanoidMap"></param>
        /// <param name="dressPartSmr"></param>
        /// <param name="baseModelHumanoidMap"></param>
        /// <param name="baseModelGameObject"></param>
        public static void RestructureBoneReferences(
            HumanoidMap dressPartHumanoidMap,
            SkinnedMeshRenderer dressPartSmr,
            HumanoidMap baseModelHumanoidMap,
            GameObject baseModelGameObject
        )
        {
            var bones = dressPartSmr.bones;
            var sharedMesh = dressPartSmr.sharedMesh;

            var validBoneIndexes = new List<int>();

            // SkinnedMeshRendererのrootBoneを素体側のHipsに差し替え
            dressPartSmr.rootBone = baseModelHumanoidMap.GetMap.FlipKvp()[HumanBodyBones.Hips];

            // ウェイトが塗られているボーンのインデックスだけを抽出
            var weightedBoneIndexes = ExtractWeightedBoneIndexes(sharedMesh);
            validBoneIndexes.AddRange(weightedBoneIndexes);

            // HumanoidBoneは素体側の参照に置き換え
            var remappedResult = RemapHumanoidBonesToBaseModel(
                dressPartHumanoidMap,
                dressPartSmr,
                baseModelHumanoidMap,
                baseModelGameObject,
                out var humanoidBoneIndexesBaseModelHasNot
            );

            // 衣装ボーンのSkinnedMeshRendererの参照の内、Hipsの上の階層を指しているものがあれば
            // 素体のHipsの上階層へ差し替え
            RemappedRootBone(
                dressPartHumanoidMap,
                baseModelHumanoidMap,
                baseModelGameObject,
                ref remappedResult
            );

            // 素体側に存在しなかったボーンを指しているウェイトを親へ転送する
            var translatedBoneWeights = TransferHumanoidBoneWeight(
                baseModelHumanoidMap,
                remappedResult.remappedBones,
                dressPartSmr.sharedMesh.boneWeights,
                humanoidBoneIndexesBaseModelHasNot
            );

            // validBoneIndexesから、素体側に存在しなかったボーンのインデックスを抜く
            validBoneIndexes = validBoneIndexes
                .Where(
                    x => !humanoidBoneIndexesBaseModelHasNot.Select(y => y.boneIndex).Contains(x)
                )
                .Distinct()
                .ToList();

            var invalidBoneIndexes = bones
                .Select((x, index) => (x, index))
                .Where(x => !validBoneIndexes.Contains(x.index))
                .Select(x => x.index)
                .ToArray();

            // validBoneIndexesに含まれるインデックスのボーンだけを残して、Smr.bones,BindPosesを再構築
            var validBones = ExtractValidBones(
                remappedBones: remappedResult.remappedBones,
                remappedBindPoses: remappedResult.remappedBindPoses,
                validBoneIndexes
            );

            // validBoneIndexesに含まれるインデックスのボーンだけを残して、BoneWeightsを再構築
            var validBoneWeights = ExtractValidBoneWeights(
                translatedBoneWeights,
                invalidBoneIndexes
            );

            dressPartSmr.bones = validBones.validBones;
            sharedMesh.bindposes = validBones.validBindPoses;
            sharedMesh.boneWeights = validBoneWeights;
        }

        /// <summary>
        /// ウェイトが塗られているSmr.bonesの内、Humanoidであるものは素体側に差し替えを実施する
        /// </summary>
        private static (
            Transform[] remappedBones,
            Matrix4x4[] remappedBindPoses
        ) RemapHumanoidBonesToBaseModel(
            HumanoidMap dressPartHumanoidMap,
            SkinnedMeshRenderer dressPartSmr,
            HumanoidMap baseModelHumanoidMap,
            GameObject baseModelGameObject,
            out List<(
                int boneIndex,
                HumanBodyBones humanBodyBones
            )> humanoidBoneIndexesBaseModelHasNot
        )
        {
            // 素体モデルが持っていないHumanoidBoneのSkinnedMeshRenderer.bones上のインデックスの一覧
            humanoidBoneIndexesBaseModelHasNot =
                new List<(int boneIndex, HumanBodyBones humanBodyBones)>();
            var resultBones = dressPartSmr.bones.ToArray();
            var resultBindPoses = dressPartSmr.sharedMesh.bindposes.ToArray();

            var baseModelMap = baseModelHumanoidMap.GetMap.FlipKvp();
            var smrBoneToIndex = dressPartSmr.bones
                .Select((bone, index) => (bone, index))
                .ToDictionary(x => x.bone, x => x.index);

            var dressPartMap = dressPartHumanoidMap.GetMap;
            foreach (var dressPartHumanoidMapKvp in dressPartMap)
            {
                var transform = dressPartHumanoidMapKvp.Key;
                var humanBodyBones = dressPartHumanoidMapKvp.Value;
                var smrBoneIndex = smrBoneToIndex[transform];

                if (baseModelMap.TryGetValue(humanBodyBones, out var baseModelTransform))
                {
                    resultBones[smrBoneIndex] = baseModelTransform;
                    resultBindPoses[smrBoneIndex] =
                        baseModelTransform.worldToLocalMatrix
                        * baseModelGameObject.transform.localToWorldMatrix;
                }
                else
                {
                    // 素体が持たないHumanoidBoneの場合、記録し、あとで親へのウェイト転送をおこなう
                    humanoidBoneIndexesBaseModelHasNot.Add((smrBoneIndex, humanBodyBones));
                }
            }

            return (resultBones, resultBindPoses);
        }

        /// <summary>
        /// 衣装パーツのSkinnedMeshRenderer.bonesのうち、Hipsの上の階層を参照しているボーンが存在する場合、
        /// 素体側のHipsの上の階層に参照を変える
        /// </summary>
        private static void RemappedRootBone(
            HumanoidMap dressPartHumanoidMap,
            HumanoidMap baseModelHumanoidMap,
            GameObject baseModelGameObject,
            ref (Transform[] remappedBones, Matrix4x4[] remappedBindPoses) remappedBones
        )
        {
            var dressPartMap = dressPartHumanoidMap.GetMap;
            if (
                !dressPartMap.FlipKvp().TryGetValue(HumanBodyBones.Hips, out var dressHipsTransform)
            )
            {
                return;
            }

            var dressHipsParent = dressHipsTransform.parent;
            if (dressHipsParent == null)
            {
                return;
            }

            var remappedTargets = remappedBones.remappedBones
                .Select((bone, index) => (bone, index))
                .Where(x => x.bone == dressHipsParent)
                .ToArray();

            if (remappedTargets.Length == 0)
            {
                return;
            }

            // 素体モデルは必ずHumanoidであるのでHipsを必ず持つ
            var baseModelHipsParent = baseModelHumanoidMap.GetMap.FlipKvp()[
                HumanBodyBones.Hips
            ].parent;

            foreach (var remappedTarget in remappedTargets)
            {
                var targetIndex = remappedTarget.index;
                remappedBones.remappedBones[targetIndex] = baseModelHipsParent;
                remappedBones.remappedBindPoses[targetIndex] =
                    baseModelHipsParent.worldToLocalMatrix
                    * baseModelGameObject.transform.localToWorldMatrix;
            }
        }

        /// <summary>
        /// ウェイトが塗られているインデックスを抽出する
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        private static List<int> ExtractWeightedBoneIndexes(Mesh mesh)
        {
            var weightedIndexes = new List<int>();
            var boneWeights = mesh.boneWeights;
            foreach (var boneWeight in boneWeights)
            {
                weightedIndexes.Add(boneWeight.boneIndex0);
                weightedIndexes.Add(boneWeight.boneIndex1);
                weightedIndexes.Add(boneWeight.boneIndex2);
                weightedIndexes.Add(boneWeight.boneIndex3);
            }

            return weightedIndexes.Distinct().ToList();
        }

        /// <summary>
        /// HumanoidBoneの参照を素体側に移したあとのBones,BindPosesから、
        /// validBoneIndexesに含まれないインデックスを抜く
        /// - Smr.bones => 対象TransformがRemoveされる
        /// - BindPoses => 対象インデックスがRemoveされる
        /// - BoneWeights => 対象インデックスがRemoveされたあとのインデックス参照となるように再構築される
        /// </summary>
        /// <param name="remappedBindPoses"></param>
        /// <param name="validBoneIndexes"></param>
        /// <param name="remappedBones"></param>
        /// <returns></returns>
        private static (Transform[] validBones, Matrix4x4[] validBindPoses) ExtractValidBones(
            Transform[] remappedBones,
            Matrix4x4[] remappedBindPoses,
            List<int> validBoneIndexes
        )
        {
            var bones = remappedBones;
            var bindPoses = remappedBindPoses;
            var resultBones = bones.ToList();
            var resultBindPoses = bindPoses.Select(bindPose => (Matrix4x4?)bindPose).ToList();

            for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
            {
                if (!validBoneIndexes.Contains(boneIndex))
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
        /// ボーンウェイトのうち、 素体に存在しなかったHumanoidBoneを指しているボーンウェイトを親ボーンに転送する
        /// bonesTranslatedHumanoidはHumanoidBoneの移植後のボーンの一覧が入る必要がある
        /// </summary>
        /// <param name="baseModelHumanoidMap"></param>
        /// <param name="bonesTranslatedHumanoid"></param>
        /// <param name="boneWeights"></param>
        /// <param name="humanoidBoneIndexesBaseModelHasNot"></param>
        /// <returns></returns>
        private static BoneWeight[] TransferHumanoidBoneWeight(
            HumanoidMap baseModelHumanoidMap,
            Transform[] bonesTranslatedHumanoid,
            BoneWeight[] boneWeights,
            List<(int boneIndex, HumanBodyBones humanBodyBones)> humanoidBoneIndexesBaseModelHasNot
        )
        {
            var boneMap = bonesTranslatedHumanoid
                .Select((x, index) => (x, index))
                .ToDictionary(x => x.x, x => x.index);
            var resultBoneWeights = boneWeights.ToArray();
            foreach (var target in humanoidBoneIndexesBaseModelHasNot)
            {
                var targetIndex = target.boneIndex;
                var targetHumanBodyBones = target.humanBodyBones;
                for (var index = 0; index < boneWeights.Length; index++)
                {
                    var boneWeight = boneWeights[index];
                    var validParentBone = GetValidParentBoneIndex(targetHumanBodyBones);
                    if (boneWeight.boneIndex0 == targetIndex)
                    {
                        resultBoneWeights[index].boneIndex0 = validParentBone;
                    }

                    if (boneWeight.boneIndex1 == targetIndex)
                    {
                        resultBoneWeights[index].boneIndex1 = validParentBone;
                    }

                    if (boneWeight.boneIndex2 == targetIndex)
                    {
                        resultBoneWeights[index].boneIndex2 = validParentBone;
                    }

                    if (boneWeight.boneIndex3 == targetIndex)
                    {
                        resultBoneWeights[index].boneIndex3 = validParentBone;
                    }
                }
            }

            return resultBoneWeights;

            int GetValidParentBoneIndex(HumanBodyBones targetHumanBodyBones)
            {
                var parent = IOParentBoneMapper.GetValidHumanoidParent(
                    baseModelHumanoidMap,
                    targetHumanBodyBones
                );
                return boneMap[parent];
            }
        }

        /// <summary>
        /// BoneWeightのインデックス参照を詰める
        /// </summary>
        /// <param name="boneWeights"></param>
        /// <param name="invalidBoneIndexes"></param>
        /// <returns></returns>
        private static BoneWeight[] ExtractValidBoneWeights(
            BoneWeight[] boneWeights,
            int[] invalidBoneIndexes
        )
        {
            var resultBoneWeights = boneWeights.ToArray();
            var descendingInvalidBoneIndexes = invalidBoneIndexes
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
