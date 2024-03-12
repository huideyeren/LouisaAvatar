using UnityEngine.Animations;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Component.UnityConstraint.LookAt;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Component.UnityConstraint.Parent
{
    public class ParentConstraintCollectComponentPlugin
        : DefaultCollectComponentPluginBase<ParentConstraint>
    {
        public override int Order => 10;

        protected override void CopyComponent(
            UnityEngine.Transform attachTarget,
            ParentConstraint sourceComponent
        )
        {
            var result = attachTarget.gameObject.AddComponent<ParentConstraint>();
            result.constraintActive = sourceComponent.constraintActive;
            result.weight = sourceComponent.weight;
            result.locked = sourceComponent.locked;
            result.translationAtRest = sourceComponent.translationAtRest;
            result.rotationAtRest = sourceComponent.rotationAtRest;
            result.translationAxis = sourceComponent.translationAxis;
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

                result.SetTranslationOffset(i, sourceComponent.GetTranslationOffset(i));
                result.SetRotationOffset(i, sourceComponent.GetRotationOffset(i));
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
            var result = new XResourceParentConstraint(constraint, gameObjectCollector);
            return result;
        }
    }
}
