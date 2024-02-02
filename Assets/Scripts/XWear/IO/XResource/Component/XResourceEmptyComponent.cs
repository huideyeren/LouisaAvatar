using UnityEngine;

namespace XWear.IO.XResource.Component
{
    public class XResourceEmptyComponent : IXResourceComponent
    {
        public ComponentType ComponentType => ComponentType.Empty;

        public UnityEngine.Component AttachTo(GameObject attachTarget)
        {
            return null;
        }
    }
}
