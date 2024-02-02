using UnityEngine;
using XWear.IO.XResource.Component;

namespace XWear.IO.XResource.SwingParameter.PhysBone
{
    public class XResourcePhysBoneComponent : IXResourceComponent
    {
        public ComponentType ComponentType => ComponentType.SwingParameter;
        public PhysBoneParameter PhysBoneParam;

        public UnityEngine.Component AttachTo(GameObject attachTarget)
        {
            // do nothing
            // PhysBoneはXWear.IOに依存してはいけない
            // したがってPlugin側でアタッチを実施する必要がある
            return null;
        }

        public XResourcePhysBoneComponent(PhysBoneParameter physBoneParam)
        {
            PhysBoneParam = physBoneParam;
        }

        public XResourcePhysBoneComponent() { }
    }
}
