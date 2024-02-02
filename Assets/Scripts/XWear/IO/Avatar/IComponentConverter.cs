using System.ComponentModel;

namespace XWear.IO.Avatar
{
    public interface IComponentConverter
    {
        int Order { get; }
        bool Check(UnityEngine.Component component);
        void Setup(UnityEngine.Transform baseModelTransform, bool isVrmBaseModel);
        void Convert(UnityEngine.Transform baseModelTransform, UnityEngine.Component component);
        void Cleanup();
        void Complete();
    }
}
