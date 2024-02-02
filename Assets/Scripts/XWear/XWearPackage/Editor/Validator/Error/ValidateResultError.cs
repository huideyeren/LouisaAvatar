using System;

namespace XWear.XWearPackage.Editor.Validator.Error
{
    [Serializable]
    public class ValidateResultError : ValidateResult
    {
        public ValidateResultError(ValidateResultType validateResultType)
            : base(validateResultType) { }
    }
}
