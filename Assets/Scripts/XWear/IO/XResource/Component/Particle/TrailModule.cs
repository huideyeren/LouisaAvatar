using System;
using UnityEngine;

namespace XWear.IO.XResource.Component.Particle
{
    [Serializable]
    public class TrailModule
    {
        public bool enabled;
        public ParticleSystemTrailMode mode;
        public float ratio;
        public XResourceMinMaxCurve lifetime;
        public float lifetimeMultiplier;
        public float minVertexDistance;
        public bool worldSpace;
        public bool dieWithParticles;
        public ParticleSystemTrailTextureMode textureMode;
        public bool sizeAffectsWidth;
        public bool sizeAffectsLifetime;
        public bool inheritParticleColor;
        public XResourceMinMaxGradient colorOverLifetime;
        public XResourceMinMaxCurve widthOverTrail;
        public XResourceMinMaxGradient colorOverTrail;
        public bool generateLightingData;
        public float shadowBias;

        public TrailModule() { }

        public TrailModule(ParticleSystem.TrailModule from)
        {
            enabled = from.enabled;
            mode = from.mode;
            ratio = from.ratio;
            lifetime = new XResourceMinMaxCurve(from.lifetime);
            lifetimeMultiplier = from.lifetimeMultiplier;
            minVertexDistance = from.minVertexDistance;
            worldSpace = from.worldSpace;
            dieWithParticles = from.dieWithParticles;
            textureMode = from.textureMode;
            sizeAffectsWidth = from.sizeAffectsWidth;
            sizeAffectsLifetime = from.sizeAffectsLifetime;
            inheritParticleColor = from.inheritParticleColor;
            colorOverLifetime = new XResourceMinMaxGradient(from.colorOverLifetime);
            widthOverTrail = new XResourceMinMaxCurve(from.widthOverTrail);
            colorOverTrail = new XResourceMinMaxGradient(from.colorOverTrail);
            generateLightingData = from.generateLightingData;
            shadowBias = from.shadowBias;
        }

        public void SetTo(ParticleSystem.TrailModule to)
        {
            to.enabled = enabled;
            to.mode = mode;
            to.ratio = ratio;
            to.lifetime = lifetime.ToUnity();
            to.lifetimeMultiplier = lifetimeMultiplier;
            to.minVertexDistance = minVertexDistance;
            to.worldSpace = worldSpace;
            to.dieWithParticles = dieWithParticles;
            to.textureMode = textureMode;
            to.sizeAffectsWidth = sizeAffectsWidth;
            to.sizeAffectsLifetime = sizeAffectsLifetime;
            to.inheritParticleColor = inheritParticleColor;
            to.colorOverLifetime = colorOverLifetime.ToUnity();
            to.widthOverTrail = widthOverTrail.ToUnity();
            to.colorOverTrail = colorOverTrail.ToUnity();
            to.generateLightingData = generateLightingData;
            to.shadowBias = shadowBias;
        }
    }
}
