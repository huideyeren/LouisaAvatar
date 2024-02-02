using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using XWear.IO;
using XWear.XWearPackage.Editor.Validator;
using XWear.XWearPackage.Editor.Validator.Error;

namespace XWear.XWearPackage.Editor.Exporter
{
    [Serializable]
    public abstract class ExporterContent
    {
        [SerializeField]
        public HumanoidBone[] currentHumanoidBones;

        [SerializeField]
        public GameObject currentRootGameObject;

        [SerializeField]
        public Transform[] currentTransforms;

        [SerializeField]
        public Renderer[] currentRenderers;

        [SerializeReference]
        public List<ValidateResult> currentValidateResults;

        public virtual void DrawGui(ExportContext exportContext, Action onUpdate) { }

        public virtual void UpdateCurrentRootGameObject(ExportContext exportContext)
        {
            currentRootGameObject = exportContext.exportRoot;
            ValidateAndInitialize(exportContext, out currentValidateResults);
        }

        protected virtual bool ValidateAndInitialize(
            ExportContext exportContext,
            out List<ValidateResult> results
        )
        {
            results = new List<ValidateResult>();
            var commonResult = ValidateCommon();
            if (commonResult != null)
            {
                results.AddRange(commonResult);
                return false;
            }

            var extraResult = ValidateExtra(exportContext);
            if (extraResult != null)
            {
                results.AddRange(extraResult);
                return false;
            }

            results = null;
            return true;
        }

        private List<ValidateResult> ValidateCommon()
        {
            if (currentRootGameObject == null)
            {
                return new List<ValidateResult>()
                {
                    new ValidateResultError(ValidateResultType.CommonErrorMissingRootGameObject)
                };
            }

            currentTransforms = currentRootGameObject.GetComponentsInChildren<Transform>();

            if (currentTransforms == null || currentTransforms.Length == 0)
            {
                return new List<ValidateResult>()
                {
                    new ValidateResultError(ValidateResultType.CommonErrorNoHierarchy)
                };
            }

            currentRenderers = currentRootGameObject.GetComponentsInChildren<Renderer>();

            if (currentRenderers == null || currentRenderers.Length == 0)
            {
                return new List<ValidateResult>()
                {
                    new ValidateResultError(ValidateResultType.CommonErrorNoRenderer)
                };
            }

            if (
                !TryGetHumanoidBonesAndValidate(
                    out var getHumanoidBoneValidateResults,
                    out var humanoidBone
                )
            )
            {
                return getHumanoidBoneValidateResults;
            }

            currentHumanoidBones = humanoidBone;

            return null;
        }

        protected abstract bool TryGetHumanoidBonesAndValidate(
            out List<ValidateResult> validateResults,
            out HumanoidBone[] humanoidBones
        );

        protected abstract List<ValidateResult> ValidateExtra(ExportContext exportContext);
    }
}
