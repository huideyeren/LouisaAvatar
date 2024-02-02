using System;

namespace XWear.XWearPackage.Editor.Validator
{
    public interface IValidateResultFixable
    {
        FixFunctionBase[] FixActions { get; }
    }

    public class SimpleFixFunction : FixFunctionBase
    {
        public readonly Func<UnityEngine.Object[]> Function;

        public SimpleFixFunction(
            Func<UnityEngine.Object[]> function,
            FixFunctionType fixFunctionType
        )
            : base(fixFunctionType)
        {
            Function = function;
        }
    }

    public abstract class FixFunctionBase
    {
        public readonly FixFunctionType FixFunctionType;

        protected FixFunctionBase(FixFunctionType fixFunctionType)
        {
            FixFunctionType = fixFunctionType;
        }
    }

    public enum FixFunctionType
    {
        Debug,
        AutoAssignHumanoidMapComponent,
        RemoveNullBoneFromHumanoidMapComponent,
        AddAccessoryMapComponent,
        DrawAccessoryMapEditor,
    }
}
