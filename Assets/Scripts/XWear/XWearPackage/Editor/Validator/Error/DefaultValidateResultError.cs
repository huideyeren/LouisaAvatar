using System;
using UnityEngine;

namespace XWear.XWearPackage.Editor.Validator.Error
{
    [Serializable]
    public class DefaultValidateResultError
        : ValidateResultError,
            IValidateResultFixable,
            IValidateResultSelectableSource
    {
        public FixFunctionBase[] FixActions { get; }
        public GameObject Source { get; }

        public DefaultValidateResultError(
            ValidateResultType validateResultType,
            GameObject source,
            FixFunctionBase[] fixActions
        )
            : base(validateResultType)
        {
            Source = source;
            FixActions = fixActions;
        }
    }
}
