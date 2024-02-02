using System;

namespace XWear.XWearPackage.Editor.Validator
{
    [Serializable]
    public class ValidateResultOk : ValidateResult
    {
        public ValidateResultOk(ValidateResultType validateResultType)
            : base(validateResultType) { }
    }
}
