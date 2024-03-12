using UnityEngine;
using UnityEngine.Animations;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Component.UnityConstraint.LookAt;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Component.UnityConstraint.Parent
{
    public class ParentConstraintBuildComponentPlugin
        : DefaultBuildComponentPluginBase<XResourceParentConstraint>
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
            var result = attachTarget.AddComponent<ParentConstraint>();
            xResource.SetTo(result, gameObjectBuilder);

            return result;
        }
    }
}
