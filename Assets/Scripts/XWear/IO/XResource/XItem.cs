using System.Collections.Generic;
using XWear.IO.XResource.Licensing;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Texture;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource
{
    public abstract class XItem
    {
        public string Guid = "";
        public string Name = "";
        public XResourceLicense License;
        public XResourceGameObject ResourceRoot = new XResourceGameObject();
        public List<XResource> XResources = new List<XResource>();

        public List<XResourceMaterial> XResourceMaterials = new List<XResourceMaterial>();
        public List<XResourceTexture> XResourceTextures = new List<XResourceTexture>();
        public List<XResourceMaterial> DefaultXResourceMaterials = new();
    }

    public abstract class XItemInstance
    {
        public readonly XItem XItem;
        public List<IXResourceInstance> XResourceInstances;
        public List<XResourceMaterialInstance> XResourceMaterialInstances;
        public List<XResourceTextureInstance> XResourceTextureInstances;
        public string Guid { get; }

        protected XItemInstance(XItem xItem)
        {
            XItem = xItem;
            Guid = xItem.Guid;
        }
    }
}
