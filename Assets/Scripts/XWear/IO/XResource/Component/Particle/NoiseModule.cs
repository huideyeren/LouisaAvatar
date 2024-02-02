using System;
using UnityEngine;

namespace XWear.IO.XResource.Component.Particle
{
    [Serializable]
    public class NoiseModule
    {
        public bool enabled;
        public XResourceMinMaxCurve strength;
        public float strengthMultiplier;
        public float frequency;

        public XResourceMinMaxCurve scrollSpeed;
        public float scrollSpeedMultiplier;

        public bool damping;
        public int octaveCount;
        public float octaveMultiplier;
        public float octaveScale;
        public ParticleSystemNoiseQuality quality;

        public bool remapEnabled;
        public XResourceMinMaxCurve remap;
        public float remapMultiplier;

        public XResourceMinMaxCurve remapX;
        public XResourceMinMaxCurve remapY;
        public XResourceMinMaxCurve remapZ;
        public float remapXMultiplier;
        public float remapYMultiplier;
        public float remapZMultiplier;

        public XResourceMinMaxCurve positionAmount;
        public XResourceMinMaxCurve rotationAmount;
        public XResourceMinMaxCurve sizeAmount;

        public NoiseModule() { }

        public NoiseModule(ParticleSystem.NoiseModule from)
        {
            enabled = from.enabled;
            strength = new XResourceMinMaxCurve(from.strength);
            strengthMultiplier = from.strengthMultiplier;
            frequency = from.frequency;
            scrollSpeed = new XResourceMinMaxCurve(from.scrollSpeed);
            scrollSpeedMultiplier = from.scrollSpeedMultiplier;
            damping = from.damping;
            octaveCount = from.octaveCount;
            octaveMultiplier = from.octaveMultiplier;
            octaveScale = from.octaveScale;
            quality = from.quality;
            remapEnabled = from.remapEnabled;
            remap = new XResourceMinMaxCurve(from.remap);
            remapMultiplier = from.remapMultiplier;
            remapX = new XResourceMinMaxCurve(from.remapX);
            remapY = new XResourceMinMaxCurve(from.remapY);
            remapZ = new XResourceMinMaxCurve(from.remapZ);
            remapXMultiplier = from.remapXMultiplier;
            remapYMultiplier = from.remapYMultiplier;
            remapZMultiplier = from.remapZMultiplier;
            positionAmount = new XResourceMinMaxCurve(from.positionAmount);
            rotationAmount = new XResourceMinMaxCurve(from.rotationAmount);
            sizeAmount = new XResourceMinMaxCurve(from.sizeAmount);
        }

        public void SetTo(ParticleSystem.NoiseModule to)
        {
            to.enabled = enabled;
            to.strength = strength.ToUnity();
            to.strengthMultiplier = strengthMultiplier;
            to.frequency = frequency;
            to.scrollSpeed = scrollSpeed.ToUnity();
            to.scrollSpeedMultiplier = scrollSpeedMultiplier;
            to.damping = damping;
            to.octaveCount = octaveCount;
            to.octaveMultiplier = octaveMultiplier;
            to.octaveScale = octaveScale;
            to.quality = quality;
            to.remapEnabled = remapEnabled;
            to.remap = remap.ToUnity();
            to.remapMultiplier = remapMultiplier;
            to.remapX = remapX.ToUnity();
            to.remapY = remapY.ToUnity();
            to.remapZ = remapZ.ToUnity();
            to.remapXMultiplier = remapXMultiplier;
            to.remapYMultiplier = remapYMultiplier;
            to.remapZMultiplier = remapZMultiplier;
            to.positionAmount = positionAmount.ToUnity();
            to.rotationAmount = rotationAmount.ToUnity();
            to.sizeAmount = sizeAmount.ToUnity();
        }
    }
}
