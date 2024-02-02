using System;
using UnityEngine;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Component.Particle
{
    [Serializable]
    public class TriggerModule
    {
        public bool enabled;
        public int maxColliderCount;
        public string[] colliderGuids = Array.Empty<string>();
        public ParticleSystemOverlapAction inside;
        public ParticleSystemOverlapAction outside;
        public ParticleSystemOverlapAction enter;
        public ParticleSystemOverlapAction exit;
        public float radiusScale;

        public TriggerModule() { }

        public TriggerModule(
            ParticleSystem.TriggerModule from,
            GameObjectWithTransformCollector gameObjectCollector
        )
        {
            enabled = from.enabled;
#if UNITY_2021_1_OR_NEWER
            maxColliderCount = from.colliderCount;
#else
            maxColliderCount = from.maxColliderCount;
#endif

            colliderGuids = new string[maxColliderCount];
            for (int i = 0; i < maxColliderCount; i++)
            {
                var collider = from.GetCollider(i);
                if (collider != null)
                {
                    colliderGuids[i] = gameObjectCollector.GetXResourceGameObjectGuidFromTransform(
                        collider.transform
                    );
                }
            }

            inside = from.inside;
            outside = from.outside;
            enter = from.enter;
            exit = from.exit;
            radiusScale = from.radiusScale;
        }

        public static void CopyUnityComponent(
            ParticleSystem.TriggerModule from,
            ParticleSystem.TriggerModule to
        )
        {
            to.enabled = from.enabled;
            var colliderCount = 0;
#if UNITY_2021_1_OR_NEWER
            colliderCount = from.colliderCount;
#else
            colliderCount = from.maxColliderCount;
#endif
            for (int i = 0; i < colliderCount; i++)
            {
                to.SetCollider(i, from.GetCollider(i));
            }

            to.inside = from.inside;
            to.outside = from.outside;
            to.enter = from.enter;
            to.exit = from.exit;
            to.radiusScale = from.radiusScale;
        }

        public void SetTo(
            ParticleSystem.TriggerModule to,
            GameObjectWithTransformBuilder gameObjectBuilder
        )
        {
            to.enabled = enabled;
            for (int i = 0; i < maxColliderCount; i++)
            {
                if (string.IsNullOrEmpty(colliderGuids[i]))
                {
                    continue;
                }

                var colliderTransform = gameObjectBuilder.GetBuildTransformFromGuid(
                    colliderGuids[i]
                );

                var collider = colliderTransform.GetComponent<UnityEngine.Collider>();
                if (collider != null)
                {
                    to.SetCollider(i, collider);
                }
            }

            to.inside = inside;
            to.outside = outside;
            to.enter = enter;
            to.exit = exit;
            to.radiusScale = radiusScale;
        }
    }
}
