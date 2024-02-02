using UnityEngine;

namespace XWear.IO.XResource.Component
{
    public interface IXResourceComponent
    {
        ComponentType ComponentType { get; }
        UnityEngine.Component AttachTo(GameObject attachTarget);
    }
}
