using UnityEngine;
using XWear.IO.XResource.Component;

namespace XWear.IO.XResource.SwingParameter.PhysBone
{
    public class XResourcePhysBoneCollider : IXResourceComponent
    {
        public ComponentType ComponentType => ComponentType.SwingColliderParameter;
        public PhysBoneColliderParameter PhysBoneColliderParameter;

        public UnityEngine.Component AttachTo(GameObject attachTarget)
        {
            // do nothing
            // PhysBoneはXWear.IOに依存してはいけない
            // したがってPlugin側でアタッチを実施する必要がある
            return null;
        }
    }
}
