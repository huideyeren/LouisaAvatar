using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XWear.IO;
using XWear.XWearPackage.Editor.Util.ExtraDrawer;
using XWear.XWearPackage.Editor.Validator;
using XWear.XWearPackage.Editor.Validator.Common;
using XWear.XWearPackage.Editor.Validator.Error;

namespace XWear.XWearPackage.Editor.Exporter
{
    [Serializable]
    public class AvatarExporterContent : ExporterContent
    {
        [SerializeReference]
        private AvatarAdvancedDrawer avatarAdvancedDrawer = new();

        public override void DrawGui(ExportContext exportContext, Action onUpdate)
        {
            base.DrawGui(exportContext, onUpdate);
            avatarAdvancedDrawer.Draw(exportContext.exportChildren, onUpdate);
        }

        protected override bool TryGetHumanoidBonesAndValidate(
            out List<ValidateResult> validateResults,
            out HumanoidBone[] humanoidBones
        )
        {
            humanoidBones = null;
            var rootObjectValidator = new Validator.Avatar.RootObjectValidator(
                currentRootGameObject
            );

            validateResults = rootObjectValidator.Check();
            if (
                validateResults
                    .Select(x => x.ValidateResultType)
                    .Any(x => x != ValidateResultType.Ok)
            )
            {
                return false;
            }

            var resultHumanoidBones = new List<HumanoidBone>();
            // rootObjectValidator.Checkが通っていればAnimatorはアタッチされているが、念のためのnullチェック
            if (rootObjectValidator.Animator != null)
            {
                foreach (HumanBodyBones humanBodyBones in Enum.GetValues(typeof(HumanBodyBones)))
                {
                    if (humanBodyBones == HumanBodyBones.LastBone)
                    {
                        continue;
                    }

                    var humanoidBone = rootObjectValidator.Animator.GetBoneTransform(
                        humanBodyBones
                    );
                    if (humanoidBone != null)
                    {
                        resultHumanoidBones.Add(
                            new HumanoidBone()
                            {
                                bone = humanoidBone,
                                humanBodyBones = humanBodyBones
                            }
                        );
                    }
                }
            }

            humanoidBones = resultHumanoidBones.ToArray();
            return true;
        }

        protected override List<ValidateResult> ValidateExtra(ExportContext exportContext)
        {
            var result = new List<ValidateResult>();

            var smrs = currentRenderers.OfType<SkinnedMeshRenderer>().ToArray();
            if (smrs.Length == 0)
            {
                result.Add(new ValidateResultError(ValidateResultType.SmrErrorNotFound));
                return result;
            }

            var smrValidator = new Validator.Common.SkinnedMeshRendererValidator(
                humanoidBones: currentHumanoidBones,
                transforms: currentTransforms,
                smrs
            );

            var children = exportContext.exportChildren;
            foreach (var child in children)
            {
                if (child == null)
                {
                    continue;
                }

                var humanoidMapValidator = new HumanoidMapComponentValidator(child);
                var checkResult = humanoidMapValidator.Check();
                if (checkResult.OfType<ValidateResultError>().Any())
                {
                    return checkResult;
                }
            }

            result.AddRange(smrValidator.Check());

            return result;
        }
    }
}
