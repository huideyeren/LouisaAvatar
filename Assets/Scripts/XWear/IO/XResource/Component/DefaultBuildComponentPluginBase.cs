using UnityEngine;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Component.ComponentPlugin;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Component
{
    public abstract class DefaultBuildComponentPluginBase<T> : IBuildComponentPlugin
        where T : IXResourceComponent
    {
        protected readonly PluginContext<T> Context = new PluginContext<T>();

        public abstract int Order { get; }

        public bool CheckIsValid(IXResourceComponent sourceComponent)
        {
            return sourceComponent is T;
        }

        public bool TrySetToContext(IXResourceComponent sourceComponent)
        {
            if (sourceComponent is T validComponent)
            {
                Context.Set(validComponent);
                return true;
            }

            return false;
        }

        public abstract UnityEngine.Component BuildAndAttach(
            GameObjectWithTransformBuilder gameObjectBuilder,
            SkinnedMeshRendererDataBuilder skinnedMeshRendererBuilder,
            MaterialBuilderBase materialBuilder,
            GameObject attachTarget,
            XResourceContainerUtil.XResourceOpener opener,
            AssetSaver assetSaver
        );
    }
}
