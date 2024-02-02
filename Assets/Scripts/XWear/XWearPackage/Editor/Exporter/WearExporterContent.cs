using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XWear.IO;
using XWear.XWearPackage.Editor.Util.ExtraDrawer;
using XWear.XWearPackage.Editor.Validator;
using XWear.XWearPackage.Editor.Validator.Error;

namespace XWear.XWearPackage.Editor.Exporter
{
    [Serializable]
    public class WearExporterContent : ExporterContent
    {
        [SerializeReference]
        private HumanoidMappingEditorDrawer humanoidMapDrawer = new();

        [SerializeField]
        private HumanoidMapComponent currentHumanoidMapComponent;

        public override void DrawGui(ExportContext exportContext, Action onUpdate)
        {
            base.DrawGui(exportContext, onUpdate);
            if (currentRootGameObject != null)
            {
                humanoidMapDrawer.DrawBody(currentHumanoidMapComponent);
            }
        }

        public override void UpdateCurrentRootGameObject(ExportContext exportContext)
        {
            var rootGameObject = exportContext.exportRoot;
            base.UpdateCurrentRootGameObject(exportContext);
            if (rootGameObject != null)
            {
                currentHumanoidMapComponent = rootGameObject.GetComponent<HumanoidMapComponent>();
            }
        }

        protected override bool TryGetHumanoidBonesAndValidate(
            out List<ValidateResult> validateResults,
            out HumanoidBone[] humanoidBones
        )
        {
            humanoidBones = null;
            var rootObjectValidator = new Validator.Wear.RootObjectValidator(
                currentRootGameObject,
                currentTransforms
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

            // rootObjectValidator.Checkが通っていれば正常なHumanoidMapComponentがアタッチされているが、念のためのnullチェック
            if (rootObjectValidator.HumanoidMapComponent != null)
            {
                humanoidBones = rootObjectValidator.HumanoidMapComponent.HumanoidMap.GetMap
                    .Select(x => new HumanoidBone() { bone = x.Key, humanBodyBones = x.Value })
                    .ToArray();
            }

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

            result.AddRange(smrValidator.Check());

            return result;
        }
    }
}
