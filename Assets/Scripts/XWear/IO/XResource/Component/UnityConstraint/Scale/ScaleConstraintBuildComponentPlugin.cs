using UnityEngine;
using UnityEngine.Animations;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Component.UnityConstraint.Scale
{
    public class ScaleConstraintBuildComponentPlugin
        : DefaultBuildComponentPluginBase<XResourceScaleConstraint>
    {
        public override int Order => 10;

        public override UnityEngine.Component BuildAndAttach(
            GameObjectWithTransformBuilder gameObjectBuilder,
            SkinnedMeshRendererDataBuilder skinnedMeshRendererBuilder,
            MaterialBuilderBase materialBuilder,
            GameObject attachTarget,
            XResourceContainerUtil.XResourceOpener opener,
            AssetSaver assetSaver
        )
        {
            var xResource = Context.Get();
            var result = attachTarget.AddComponent<ScaleConstraint>();
            xResource.SetTo(result, gameObjectBuilder);

            return result;
        }
    }
}