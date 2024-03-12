using UnityEngine.Animations;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Component.UnityConstraint.Aim;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Component.UnityConstraint.Position
{
    public class PositionConstraintCollectComponentPlugin
        : DefaultCollectComponentPluginBase<PositionConstraint>
    {
        public override int Order => 10;

        protected override void CopyComponent(
            UnityEngine.Transform attachTarget,
            PositionConstraint sourceComponent
        )
        {
            var result = attachTarget.gameObject.AddComponent<PositionConstraint>();
            result.constraintActive = sourceComponent.constraintActive;
            result.weight = sourceComponent.weight;

            result.locked = sourceComponent.locked;
            result.translationAtRest = sourceComponent.translationAtRest;
            result.translationOffset = sourceComponent.translationOffset;
            result.translationAxis = sourceComponent.translationAxis;

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
            var result = new XResourcePositionConstraint(constraint, gameObjectCollector);
            return result;
        }
    }
}
