using System.Collections.Generic;

namespace XWear.XWearPackage.Editor.Validator
{
    public interface IXResourceValidator
    {
        List<ValidateResult> Check();
    }
}
