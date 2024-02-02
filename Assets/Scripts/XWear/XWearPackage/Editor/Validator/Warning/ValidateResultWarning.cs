using System;

namespace XWear.XWearPackage.Editor.Validator.Warning
{
    [Serializable]
    public abstract class ValidateResultWarning : ValidateResult
    {
        protected ValidateResultWarning(ValidateResultType validateResultType)
            : base(validateResultType) { }
    }
}
