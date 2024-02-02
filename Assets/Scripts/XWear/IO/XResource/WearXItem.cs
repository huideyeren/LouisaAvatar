using System.Collections.Generic;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Texture;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource
{
    public class WearXItem : XItem
    {
        public WearXItem(XItem xItem)
        {
            ResourceRoot = xItem.ResourceRoot;
            XResources = xItem.XResources;
            XResourceMaterials = xItem.XResourceMaterials;
            XResourceTextures = xItem.XResourceTextures;
        }

        public WearXItem() { }
    }

    public class WearXItemInstance : XItemInstance
    {
        public WearXItemInstance(WearXItem xItem)
            : base(xItem) { }
    }
}
