using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using XWear.IO;
using XWear.XWearPackage.Editor.Util;
using XWear.XWearPackage.Editor.Validator.Error;
using XWear.XWearPackage.Editor.Validator.Warning;

namespace XWear.XWearPackage.Editor.Validator.Common
{
    public class SkinnedMeshRendererValidator : IXResourceValidator
    {
        private readonly Transform[] _humanoidBoneTransforms;
        private readonly Transform _hipsTransform;
        private readonly Transform[] _rootBones;
        private readonly Transform[] _transforms;
        private readonly SkinnedMeshRenderer[] _skinnedMeshRenderers;

        private readonly Dictionary<
            SkinnedMeshRenderer,
            (List<Transform> weightedBones, List<Transform> zeroWeightedBones)
        > _weightInfoMap = new();

        public SkinnedMeshRendererValidator(
            HumanoidBone[] humanoidBones,
            Transform[] transforms,
            SkinnedMeshRenderer[] skinnedMeshRenderers
        )
        {
            _humanoidBoneTransforms = humanoidBones.Select(x => x.bone).ToArray();
            _hipsTransform = humanoidBones
                .FirstOrDefault(x => x.humanBodyBones == HumanBodyBones.Hips)
                ?.bone;
            _transforms = transforms;
            _skinnedMeshRenderers = skinnedMeshRenderers;
            _rootBones = _skinnedMeshRenderers.Select(x => x.rootBone).ToArray();
            foreach (var smr in _skinnedMeshRenderers)
            {
                _weightInfoMap.Add(smr, MeshUtil.GetWeightInfo(smr, smr.sharedMesh));
            }
        }

        public List<ValidateResult> Check()
        {
            if (!CheckRootBone(out var checkRootBoneResult))
            {
                return new List<ValidateResult> { checkRootBoneResult };
            }

            if (!CheckMeshBones(out var checkMeshBonesResult))
            {
                return checkMeshBonesResult;
            }

            return new List<ValidateResult> { new ValidateResultOk(ValidateResultType.Ok) };
        }

        private bool CheckRootBone([NotNullWhen(false)] out ValidateResult result)
        {
            foreach (var skinnedMeshRenderer in _skinnedMeshRenderers)
            {
                var rootBone = skinnedMeshRenderer.rootBone;
                if (rootBone == null)
                {
                    // RootBoneが設定されているか
                    result = new SelectableValidateResultError(
                        ValidateResultType.SmrErrorMissingRootBone,
                        skinnedMeshRenderer.gameObject
                    );
                    return false;
                }

                if (!_transforms.Contains(rootBone))
                {
                    // RootBoneが出力対象のTransform一覧に含まれるか
                    result = new SelectableValidateResultError(
                        ValidateResultType.SmrErrorNotContainsRootBone,
                        skinnedMeshRenderer.gameObject
                    );

                    return false;
                }
            }

            result = null;
            return true;
        }

        private bool CheckMeshBones([NotNullWhen(false)] out List<ValidateResult> result)
        {
            result = new List<ValidateResult>();
            foreach (var skinnedMeshRenderer in _skinnedMeshRenderers)
            {
                var bones = skinnedMeshRenderer.bones;
                var weightInfo = _weightInfoMap[skinnedMeshRenderer];

                if (
                    ContainsInactiveWeightedBones(
                        bones,
                        weightInfo.weightedBones,
                        out var invalidBone
                    )
                )
                {
                    // いずれかのメッシュ上でウェイトが塗られているが非アクティブなボーンを含んでいる
                    result.Clear();
                    result = new List<ValidateResult>()
                    {
                        new SelectableValidateResultError(
                            ValidateResultType.SmrErrorInactiveWeightedBones,
                            invalidBone.gameObject
                        )
                    };
                    return false;
                }

                if (skinnedMeshRenderer.bones.Contains(null))
                {
                    // Nullなボーンを含んでいる
                    result.Add(
                        new SelectableValidateResultWarning(
                            ValidateResultType.SmrWarningContainsNulBone,
                            skinnedMeshRenderer.gameObject
                        )
                    );
                }
            }

            if (ContainsZeroWeightBones(out var zeroWeightBone))
            {
                // どのメッシュからもウェイトが塗られていないボーンを含んでいる
                result.Add(
                    new SelectableValidateResultWarning(
                        ValidateResultType.SmrWarningZeroWeightBone,
                        zeroWeightBone.gameObject
                    )
                );
            }

            if (result.Count > 0)
            {
                return false;
            }

            result = null;
            return true;
        }

        private bool ContainsInactiveWeightedBones(
            Transform[] bones,
            List<Transform> weightedBones,
            out Transform invalidBone
        )
        {
            invalidBone = null;
            for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
            {
                var bone = bones[boneIndex];
                if (bone == null)
                {
                    continue;
                }

                if (!weightedBones.Contains(bone))
                {
                    continue;
                }

                if (!bone.gameObject.activeInHierarchy)
                {
                    invalidBone = bone;
                    return true;
                }
            }

            return false;
        }

        private bool ContainsZeroWeightBones(out Transform resultZeroWeightedBone)
        {
            resultZeroWeightedBone = null;

            var allWeightedBones = _weightInfoMap
                .SelectMany(x => x.Value.weightedBones)
                .Distinct()
                .ToList();
            var allZeroWeightedBone = _weightInfoMap
                .SelectMany(x => x.Value.zeroWeightedBones)
                .Where(x => x != null)
                .Distinct()
                .ToList();

            foreach (var zeroWeightedBone in allZeroWeightedBone)
            {
                if (allWeightedBones.Contains(zeroWeightedBone))
                {
                    continue;
                }

                // HumanoidBoneは無視
                if (_humanoidBoneTransforms.Contains(zeroWeightedBone))
                {
                    continue;
                }

                // Rootになっている親は抜きたくないので親がHipsの親と同じであれば無視
                if (_hipsTransform != null)
                {
                    if (ReferenceEquals(_hipsTransform.parent, zeroWeightedBone))
                    {
                        continue;
                    }
                }

                // SkinnedMeshRendererのRootBoneは抜きたくないので無視
                if (_rootBones.Contains(zeroWeightedBone))
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
                    if (_humanoidBoneTransforms.Contains(childBone))
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
                // ContainsInactiveWeightedBonesによるチェックによって、エラーとして返却されている
                if (!zeroWeightedBone.gameObject.activeInHierarchy)
                {
                    continue;
                }

                // どのメッシュ上でもウェイトが塗られていないボーンが見つかれば返却
                resultZeroWeightedBone = zeroWeightedBone;
                return true;
            }

            return false;
        }
    }
}
