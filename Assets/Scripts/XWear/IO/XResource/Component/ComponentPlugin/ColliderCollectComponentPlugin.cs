using System;
using UnityEngine;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Component.Collider;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Component.ComponentPlugin
{
    public class ColliderCollectComponentPlugin
        : DefaultCollectComponentPluginBase<UnityEngine.Collider>
    {
        public override int Order => 10;

        protected override void CopyComponent(
            UnityEngine.Transform attachTarget,
            UnityEngine.Collider sourceComponent
        )
        {
            if (sourceComponent is BoxCollider boxCollider)
            {
                var attached = attachTarget.gameObject.AddComponent<BoxCollider>();
                var tmp = new XResourceBoxCollider(boxCollider);
                tmp.SetTo(attached);
            }
            else if (sourceComponent is SphereCollider sphereCollider)
            {
                var attached = attachTarget.gameObject.AddComponent<SphereCollider>();
                var tmp = new XResourceSphereCollider(sphereCollider);
                tmp.SetTo(attached);
            }
            else if (sourceComponent is CapsuleCollider capsuleCollider)
            {
                var attached = attachTarget.gameObject.AddComponent<CapsuleCollider>();
                var tmp = new XResourceCapsuleCollider(capsuleCollider);
                tmp.SetTo(attached);
            }
            else
            {
                var attached = attachTarget.gameObject.AddComponent<BoxCollider>();
                attached.enabled = sourceComponent.enabled;
                var b = attached.bounds;
                var sourceBounds = sourceComponent.bounds;
                b.center = sourceBounds.center;
                b.extents = sourceBounds.extents;
                b.max = sourceBounds.max;
                b.min = sourceBounds.min;

                Debug.LogWarning(
                    $"{sourceComponent.GetType()} is not supported Collider. Fallback to BoxCollider."
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
            var context = Context.Get();
            if (context is BoxCollider boxCollider)
            {
                return new XResourceBoxCollider(boxCollider);
            }

            if (context is SphereCollider sphereCollider)
            {
                return new XResourceSphereCollider(sphereCollider);
            }

            if (context is CapsuleCollider capsuleCollider)
            {
                return new XResourceCapsuleCollider(capsuleCollider);
            }

            throw new NotImplementedException();
        }
    }
}
