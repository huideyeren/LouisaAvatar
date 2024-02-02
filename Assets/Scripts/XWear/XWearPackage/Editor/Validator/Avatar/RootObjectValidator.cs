using System;
using System.Collections.Generic;
using UnityEngine;
using XWear.XWearPackage.Editor.Validator.Error;

namespace XWear.XWearPackage.Editor.Validator.Avatar
{
    public class RootObjectValidator : IXAvatarValidator
    {
        private readonly GameObject _rootGameObject;
        public Animator Animator { get; private set; }

        public RootObjectValidator(GameObject rootGameObject)
        {
            _rootGameObject = rootGameObject;
        }

        public List<ValidateResult> Check()
        {
            var results = new List<ValidateResult>();
            // .xavatar出力時はHumanoidなAnimatorが必須
            // HumanoidComponentは自動でアタッチされるので、Animatorのチェックだけを実施する
            results.Add(CheckAnimator(out var animator));
            Animator = animator;

            // 同名オブジェクトが含まれている
            // ランタイムでAvatarBuilder.BuildHumanAvatarを通さないといけない関係上、
            // 同名オブジェクトが含まれることを許容できない
            if (Animator != null)
            {
                if (CheckContainsHumanoidSameNameObject(Animator, out var sameNameTransform))
                {
                    results.Add(
                        new SelectableValidateResultError(
                            ValidateResultType.AvatarErrorContainsSameNameHierarchy,
                            sameNameTransform.gameObject
                        )
                    );
                }
            }

            return results;
        }

        private ValidateResult CheckAnimator(out Animator animator)
        {
            animator = _rootGameObject.GetComponent<Animator>();
            if (animator == null)
            {
                // Animatorがアタッチされていない
                return new SelectableValidateResultError(
                    ValidateResultType.AvatarErrorNotFoundAnimator,
                    _rootGameObject
                );
            }

            if (!animator.isHuman)
            {
                // Animatorがアタッチされているが、Humanoidでない
                return new SelectableValidateResultError(
                    ValidateResultType.AvatarErrorNotHumanoid,
                    _rootGameObject
                );
            }

            return new ValidateResultOk(ValidateResultType.Ok);
        }

        private bool CheckContainsHumanoidSameNameObject(Animator animator, out Transform target)
        {
            var humanoidBones = new List<Transform>();
            var humanoidBoneNames = new List<string>();

            foreach (HumanBodyBones humanBodyBones in Enum.GetValues(typeof(HumanBodyBones)))
            {
                if (humanBodyBones == HumanBodyBones.LastBone)
                {
                    continue;
                }

                var bone = animator.GetBoneTransform(humanBodyBones);
                if (bone != null)
                {
                    humanoidBones.Add(bone);
                    humanoidBoneNames.Add(bone.gameObject.name);
                }
            }

            target = null;
            var transforms = _rootGameObject.GetComponentsInChildren<Transform>(true);
            foreach (var transform in transforms)
            {
                if (humanoidBones.Contains(transform))
                {
                    continue;
                }

                if (humanoidBoneNames.Contains(transform.gameObject.name))
                {
                    target = transform;
                    return true;
                }
            }

            return false;
        }
    }
}
