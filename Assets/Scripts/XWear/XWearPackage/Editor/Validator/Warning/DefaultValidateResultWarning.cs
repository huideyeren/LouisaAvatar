using System;
using UnityEngine;

namespace XWear.XWearPackage.Editor.Validator.Warning
{
    [Serializable]
    public class DefaultValidateResultWarning
        : ValidateResultWarning,
            IValidateResultSelectableSource,
            IValidateResultFixable
    {
        public FixFunctionBase[] FixActions { get; }
        public GameObject Source { get; }

        public DefaultValidateResultWarning(
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
