using System;
using UnityEngine;

namespace XWear.XWearPackage.Editor.Validator.Warning
{
    [Serializable]
    public class SelectableValidateResultWarning
        : ValidateResultWarning,
            IValidateResultSelectableSource
    {
        public GameObject Source { get; }

        public SelectableValidateResultWarning(
            ValidateResultType validateResultType,
            GameObject source
        )
            : base(validateResultType)
        {
            Source = source;
        }
    }
}
