using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Rendering;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.Component.Particle
{
    [Serializable]
    public class XResourceParticleRenderer
    {
        public ParticleSystemRenderMode renderMode;
        public float normalDirection;
        public string materialGuid;
        public string trailMaterialGuid;
        public ParticleSystemSortMode sortMode;
        public float sortingFudge;
        public float minParticleSize;
        public float maxParticleSize;
        public ParticleSystemRenderSpace alignment;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 flip;

        public bool allowRoll;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 pivot;

        public SpriteMaskInteraction maskInteraction;
        public ShadowCastingMode shadowCastingMode;
        public bool receiveShadows;
        public float shadowBias;
        public MotionVectorGenerationMode motionVectorGenerationMode;
        public int sortingLayerID;
        public int sortingOrder;
        public LightProbeUsage lightProbeUsage;
        public ReflectionProbeUsage reflectionProbeUsage;

        public XResourceParticleRenderer() { }

        public XResourceParticleRenderer(
            ParticleSystemRenderer renderer,
            MaterialCollectorBase materialCollector
        )
        {
            renderMode = renderer.renderMode;
            normalDirection = renderer.normalDirection;
            if (renderer.sharedMaterial != null)
            {
                materialGuid = materialCollector.TryCollectAndAdd(renderer.sharedMaterial).Guid;
            }

            if (renderer.trailMaterial != null)
            {
                trailMaterialGuid = materialCollector.TryCollectAndAdd(renderer.trailMaterial).Guid;
            }

            sortMode = renderer.sortMode;
            sortingFudge = renderer.sortingFudge;
            minParticleSize = renderer.minParticleSize;
            maxParticleSize = renderer.maxParticleSize;
            alignment = renderer.alignment;
            flip = renderer.flip;
            allowRoll = renderer.allowRoll;
            pivot = renderer.pivot;
            maskInteraction = renderer.maskInteraction;
            shadowCastingMode = renderer.shadowCastingMode;
            receiveShadows = renderer.receiveShadows;
            shadowBias = renderer.shadowBias;
            motionVectorGenerationMode = renderer.motionVectorGenerationMode;

            sortingLayerID = renderer.sortingLayerID;
            sortingOrder = renderer.sortingOrder;
            lightProbeUsage = renderer.lightProbeUsage;
            reflectionProbeUsage = renderer.reflectionProbeUsage;
        }

        public static void CopyUnityComponent(
            ParticleSystemRenderer from,
            ParticleSystemRenderer to
        )
        {
            to.renderMode = from.renderMode;
            to.normalDirection = from.normalDirection;
            to.sharedMaterial = from.sharedMaterial;
            to.trailMaterial = from.trailMaterial;

            to.sortMode = from.sortMode;
            to.sortingFudge = from.sortingFudge;
            to.minParticleSize = from.minParticleSize;
            to.maxParticleSize = from.maxParticleSize;
            to.alignment = from.alignment;
            to.flip = from.flip;
            to.allowRoll = from.allowRoll;
            to.pivot = from.pivot;
            to.maskInteraction = from.maskInteraction;
            to.shadowCastingMode = from.shadowCastingMode;
            to.receiveShadows = from.receiveShadows;
            to.shadowBias = from.shadowBias;
            to.motionVectorGenerationMode = from.motionVectorGenerationMode;

            to.sortingLayerID = from.sortingLayerID;
            to.sortingOrder = from.sortingOrder;
            to.lightProbeUsage = from.lightProbeUsage;
            to.reflectionProbeUsage = from.reflectionProbeUsage;
        }

        public void SetTo(ParticleSystemRenderer to, MaterialBuilderBase materialBuilder)
        {
            to.renderMode = renderMode;
            to.normalDirection = normalDirection;

            if (!string.IsNullOrEmpty(materialGuid))
            {
                materialBuilder.TryGetMaterialFromGuid(materialGuid, out var mat);
                to.sharedMaterial = mat;
            }

            if (!string.IsNullOrEmpty(trailMaterialGuid))
            {
                if (materialBuilder.TryGetMaterialFromGuid(trailMaterialGuid, out var trailMat))
                {
                    to.trailMaterial = trailMat;
                }
            }

            to.sortMode = sortMode;
            to.sortingFudge = sortingFudge;
            to.minParticleSize = minParticleSize;
            to.maxParticleSize = maxParticleSize;
            to.alignment = alignment;
            to.flip = flip;
            to.allowRoll = allowRoll;
            to.pivot = pivot;
            to.maskInteraction = maskInteraction;
            to.shadowCastingMode = shadowCastingMode;
            to.receiveShadows = receiveShadows;
            to.shadowBias = shadowBias;
            to.motionVectorGenerationMode = motionVectorGenerationMode;

            to.sortingLayerID = sortingLayerID;
            to.sortingOrder = sortingOrder;
            to.lightProbeUsage = lightProbeUsage;
            to.reflectionProbeUsage = reflectionProbeUsage;
        }
    }
}
