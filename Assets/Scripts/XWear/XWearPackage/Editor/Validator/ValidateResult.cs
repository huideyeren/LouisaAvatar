using System;

namespace XWear.XWearPackage.Editor.Validator
{
    [Serializable]
    public abstract class ValidateResult
    {
        public ValidateResultType ValidateResultType { get; }

        protected ValidateResult(ValidateResultType validateResultType)
        {
            ValidateResultType = validateResultType;
        }
    }
}
