using System.Collections.Generic;
using System.Linq;
using XWear.IO.XResource;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Texture;

namespace XWear.IO.Editor.Material
{
    public class EditorMaterialCollector : MaterialCollectorBase
    {
        public EditorMaterialCollector(TextureCollectorBase textureCollector)
            : base(textureCollector) { }

        public override XResourceMaterial TryCollectAndAdd(UnityEngine.Material sourceMaterial)
        {
            if (MaterialToXResourceMaterialMemo.TryGetValue(sourceMaterial, out var collected))
            {
                return collected;
            }

            return CollectAsNew(sourceMaterial);
        }

        public override void CollectDefaultMaterials(XItem xItem)
        {
            xItem.DefaultXResourceMaterials = CurrentContextReferenced.ToList();
        }
    }
}
