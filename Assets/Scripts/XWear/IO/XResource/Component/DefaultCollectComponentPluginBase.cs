using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Component.ComponentPlugin;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Component
{
    public abstract class DefaultCollectComponentPluginBase<T> : ICollectComponentPlugin
        where T : UnityEngine.Component
    {
        protected readonly PluginContext<T> Context = new PluginContext<T>();

        public abstract int Order { get; }

        public virtual bool CheckIsValid(UnityEngine.Component attachSource)
        {
            return attachSource is T;
        }

        public void CopyComponent(
            UnityEngine.Transform attachTarget,
            UnityEngine.Component sourceComponent
        )
        {
            if (sourceComponent is T component)
            {
                CopyComponent(attachTarget, component);
            }
        }

        protected abstract void CopyComponent(
            UnityEngine.Transform attachTarget,
            T sourceComponent
        );

        public bool TrySetToContext(UnityEngine.Component component)
        {
            if (component is T validComponent)
            {
                Context.Set(validComponent);
                return true;
            }

            return false;
        }

        public abstract IXResourceComponent Collect(
            XItem xItem,
            MaterialCollectorBase materialCollector,
            GameObjectWithTransformCollector gameObjectCollector,
            SkinnedMeshRendererDataCollector skinnedMeshRendererCollector,
            XResourceContainerUtil.XResourceArchiver archiver
        );
    }
}
