using System;

namespace XWear.XWearPackage.Editor.Validator.Warning
{
    [Serializable]
    public class FixableValidateResultWarning : ValidateResultWarning, IValidateResultFixable
    {
        public FixFunctionBase[] FixActions { get; }

        public FixableValidateResultWarning(
            ValidateResultType validateResultType,
            FixFunctionBase[] fixActions
        )
            : base(validateResultType)
        {
            FixActions = fixActions;
        }
    }
}
