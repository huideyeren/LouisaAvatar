using UnityEngine.Animations;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Component.UnityConstraint.LookAt
{
    public class LookAtConstraintCollectComponentPlugin
        : DefaultCollectComponentPluginBase<LookAtConstraint>
    {
        public override int Order => 10;

        protected override void CopyComponent(
            UnityEngine.Transform attachTarget,
            LookAtConstraint sourceComponent
        )
        {
            var result = attachTarget.gameObject.AddComponent<LookAtConstraint>();
            result.constraintActive = sourceComponent.constraintActive;
            result.weight = sourceComponent.weight;
            result.useUpObject = sourceComponent.useUpObject;
            result.roll = sourceComponent.roll;
            result.worldUpObject = sourceComponent.worldUpObject;
            result.locked = sourceComponent.locked;
            result.rotationAtRest = sourceComponent.rotationAtRest;
            result.rotationOffset = sourceComponent.rotationOffset;

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
            var result = new XResourceLookAtConstraint(constraint, gameObjectCollector);
            return result;
        }
    }
}
