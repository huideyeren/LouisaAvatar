using System;
using System.Collections.Generic;
using UnityEngine;
using XWear.IO;
using XWear.XWearPackage.Editor.Validator;
using XWear.XWearPackage.Editor.Validator.Error;

namespace XWear.XWearPackage.Editor.Exporter
{
    [Serializable]
    public class AccessoryExporterContent : ExporterContent
    {
        protected override bool ValidateAndInitialize(
            ExportContext exportContext,
            out List<ValidateResult> results
        )
        {
            results = new List<ValidateResult>()
            {
                new ValidateResultError(ValidateResultType.AccessoryIsNotSupported)
            };
            return false;
        }

        protected override bool TryGetHumanoidBonesAndValidate(
            out List<ValidateResult> validateResults,
            out HumanoidBone[] humanoidBones
        )
        {
            throw new NotImplementedException();
        }

        protected override List<ValidateResult> ValidateExtra(ExportContext exportContext)
        {
            throw new NotImplementedException();
        }
    }
}
