using UnityEngine.Animations;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Component.UnityConstraint.Scale
{
    public class ScaleConstraintCollectComponentPlugin
        : DefaultCollectComponentPluginBase<ScaleConstraint>
    {
        public override int Order => 10;

        protected override void CopyComponent(
            UnityEngine.Transform attachTarget,
            ScaleConstraint sourceComponent
        )
        {
            var result = attachTarget.gameObject.AddComponent<ScaleConstraint>();
            result.constraintActive = sourceComponent.constraintActive;
            result.weight = sourceComponent.weight;

            result.locked = sourceComponent.locked;
            result.scaleAtRest = sourceComponent.scaleAtRest;
            result.scaleOffset = sourceComponent.scaleOffset;
            result.scalingAxis = sourceComponent.scalingAxis;

            for (int i = 0; i < sourceComponent.sourceCount; i++)
            {
                var source = sourceComponent.GetSource(i);
                result.AddSource(
                    new UnityEngine.Animations.ConstraintSource()
                    {
                        sourceTransform = source.sourceTransform,
                        weight = source.weight
                    }
                );
            }
        }

        public override IXResourceComponent Collect(
            XItem xItem,
            MaterialCollectorBase materialCollector,
            GameObjectWithTransformCollector gameObjectCollector,
            SkinnedMeshRendererDataCollector skinnedMeshRendererCollector,
            XResourceContainerUtil.XResourceArchiver archiver
        )
        {
            var constraint = Context.Get();
            var result = new XResourceScaleConstraint(constraint, gameObjectCollector);
            return result;
        }
    }
}
