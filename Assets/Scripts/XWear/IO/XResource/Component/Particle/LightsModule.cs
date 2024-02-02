using System;
using UnityEngine;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Component.Particle
{
    [Serializable]
    public class LightsModule
    {
        public bool enabled;
        public string lightTransformGuid;
        public float ratio;
        public bool useRandomDistribution;
        public bool useParticleColor;
        public bool sizeAffectsRange;
        public bool alphaAffectsIntensity;
        public XResourceMinMaxCurve range;
        public float rangeMultiplier;
        public XResourceMinMaxCurve intensity;
        public float intensityMultiplier;
        public int maxLights;

        public LightsModule() { }

        public LightsModule(
            ParticleSystem.LightsModule from,
            GameObjectWithTransformCollector gameObjectCollector
        )
        {
            enabled = from.enabled;
            if (from.light != null)
            {
                lightTransformGuid = gameObjectCollector.GetXResourceGameObjectGuidFromTransform(
                    from.light.transform
                );
            }

            ratio = from.ratio;
            useRandomDistribution = from.useRandomDistribution;
            useParticleColor = from.useParticleColor;
            sizeAffectsRange = from.sizeAffectsRange;
            alphaAffectsIntensity = from.alphaAffectsIntensity;
            range = new XResourceMinMaxCurve(from.range);
            rangeMultiplier = from.rangeMultiplier;
            intensity = new XResourceMinMaxCurve(from.intensity);
            intensityMultiplier = from.intensityMultiplier;
            maxLights = from.maxLights;
        }

        public static void CopyUnityComponent(
            ParticleSystem.LightsModule from,
            ParticleSystem.LightsModule to
        )
        {
            to.enabled = from.enabled;
            to.light = from.light;

            to.ratio = from.ratio;
            to.useRandomDistribution = from.useRandomDistribution;
            to.useParticleColor = from.useParticleColor;
            to.sizeAffectsRange = from.sizeAffectsRange;
            to.alphaAffectsIntensity = from.alphaAffectsIntensity;
            to.range = new XResourceMinMaxCurve(from.range).ToUnity();
            to.rangeMultiplier = from.rangeMultiplier;
            to.intensity = new XResourceMinMaxCurve(from.intensity).ToUnity();
            to.intensityMultiplier = from.intensityMultiplier;
            to.maxLights = from.maxLights;
        }

        public void SetTo(
            ParticleSystem.LightsModule to,
            GameObjectWithTransformBuilder gameObjectBuilder
        )
        {
            to.enabled = enabled;
            if (!string.IsNullOrEmpty(lightTransformGuid))
            {
                var lightTransform = gameObjectBuilder.GetBuildTransformFromGuid(
                    lightTransformGuid
                );
                var light = lightTransform.GetComponent<Light>();
                if (light != null)
                {
                    to.light = light;
                }
            }

            to.ratio = ratio;
            to.useRandomDistribution = useRandomDistribution;
            to.useParticleColor = useParticleColor;
            to.sizeAffectsRange = sizeAffectsRange;
            to.alphaAffectsIntensity = alphaAffectsIntensity;
            to.range = range.ToUnity();
            to.rangeMultiplier = rangeMultiplier;
            to.intensity = intensity.ToUnity();
            to.intensityMultiplier = intensityMultiplier;
            to.maxLights = maxLights;
        }
    }
}
