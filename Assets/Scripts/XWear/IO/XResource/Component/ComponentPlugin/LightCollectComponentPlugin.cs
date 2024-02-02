using UnityEngine;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Component.ComponentPlugin
{
    public class LightCollectComponentPlugin : ReflectionCollectComponentPlugin<Light>
    {
        public override int Order => 10;

        public override IXResourceComponent Collect(
            XItem xItem,
            MaterialCollectorBase materialCollector,
            GameObjectWithTransformCollector gameObjectCollector,
            SkinnedMeshRendererDataCollector skinnedMeshRendererCollector,
            XResourceContainerUtil.XResourceArchiver archiver
        )
        {
            var light = Context.Get();
            return new XResourceLight(light);
        }
    }
}
