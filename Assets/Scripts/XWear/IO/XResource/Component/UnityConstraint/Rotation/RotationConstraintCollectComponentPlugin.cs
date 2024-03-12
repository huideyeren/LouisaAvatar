using UnityEngine.Animations;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Component.UnityConstraint.Aim;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Component.UnityConstraint.Rotation
{
    public class RotationConstraintCollectComponentPlugin
        : DefaultCollectComponentPluginBase<RotationConstraint>
    {
        public override int Order => 10;

        protected override void CopyComponent(
            UnityEngine.Transform attachTarget,
            RotationConstraint sourceComponent
        )
        {
            var result = attachTarget.gameObject.AddComponent<RotationConstraint>();
            result.constraintActive = sourceComponent.constraintActive;
            result.weight = sourceComponent.weight;

            result.locked = sourceComponent.locked;
            result.rotationAtRest = sourceComponent.rotationAtRest;
            result.rotationOffset = sourceComponent.rotationOffset;
            result.rotationAxis = sourceComponent.rotationAxis;

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
            var result = new XResourceRotationConstraint(constraint, gameObjectCollector);
            return result;
        }
    }
}
