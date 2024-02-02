using System;
using UnityEngine;

namespace XWear.XWearPackage.Editor.Validator.Error
{
    [Serializable]
    public class SelectableValidateResultError
        : ValidateResultError,
            IValidateResultSelectableSource
    {
        public GameObject Source { get; }

        public SelectableValidateResultError(
            ValidateResultType validateResultType,
            GameObject source
        )
            : base(validateResultType)
        {
            Source = source;
        }
    }
}
