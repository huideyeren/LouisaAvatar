namespace XWear.IO.XResource
{
    public class AvatarXItem : XItem
    {
        public bool FromVrm;

        public AvatarXItem(XItem xItem)
        {
            ResourceRoot = xItem.ResourceRoot;
            XResources = xItem.XResources;
            XResourceMaterials = xItem.XResourceMaterials;
            XResourceTextures = xItem.XResourceTextures;
        }

        public AvatarXItem() { }
    }

    public class AvatarXItemInstance : XItemInstance
    {
        public AvatarXItemInstance(AvatarXItem xItem)
            : base(xItem) { }
    }
}
