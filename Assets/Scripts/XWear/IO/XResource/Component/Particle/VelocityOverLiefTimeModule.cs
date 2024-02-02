using System;
using UnityEngine;

namespace XWear.IO.XResource.Component.Particle
{
    [Serializable]
    public class VelocityOverLiefTimeModule
    {
        public bool enabled;
        public XResourceMinMaxCurve x;
        public XResourceMinMaxCurve y;
        public XResourceMinMaxCurve z;
        public float xMultiplier;
        public float yMultiplier;
        public float zMultiplier;

        public ParticleSystemSimulationSpace space;

        public XResourceMinMaxCurve orbitalX;
        public XResourceMinMaxCurve orbitalY;
        public XResourceMinMaxCurve orbitalZ;
        public float orbitalXMultiplier;
        public float orbitalYMultiplier;
        public float orbitalZMultiplier;

        public XResourceMinMaxCurve orbitalOffsetX;
        public XResourceMinMaxCurve orbitalOffsetY;
        public XResourceMinMaxCurve orbitalOffsetZ;
        public float orbitalOffsetXMultiplier;
        public float orbitalOffsetYMultiplier;
        public float orbitalOffsetZMultiplier;

        public XResourceMinMaxCurve radial;
        public float radialMultiplier;

        public XResourceMinMaxCurve speedModifier;
        public float speedModifierMultiplier;

        public VelocityOverLiefTimeModule() { }

        public VelocityOverLiefTimeModule(ParticleSystem.VelocityOverLifetimeModule from)
        {
            enabled = from.enabled;
            x = new XResourceMinMaxCurve(from.x);
            y = new XResourceMinMaxCurve(from.y);
            z = new XResourceMinMaxCurve(from.z);
            xMultiplier = from.xMultiplier;
            yMultiplier = from.yMultiplier;
            zMultiplier = from.zMultiplier;

            space = from.space;
            orbitalX = new XResourceMinMaxCurve(from.orbitalX);
            orbitalY = new XResourceMinMaxCurve(from.orbitalY);
            orbitalZ = new XResourceMinMaxCurve(from.orbitalZ);
            orbitalXMultiplier = from.orbitalXMultiplier;
            orbitalYMultiplier = from.orbitalYMultiplier;
            orbitalZMultiplier = from.orbitalZMultiplier;

            orbitalOffsetX = new XResourceMinMaxCurve(from.orbitalOffsetX);
            orbitalOffsetY = new XResourceMinMaxCurve(from.orbitalOffsetY);
            orbitalOffsetZ = new XResourceMinMaxCurve(from.orbitalOffsetZ);
            orbitalOffsetXMultiplier = from.orbitalOffsetXMultiplier;
            orbitalOffsetYMultiplier = from.orbitalOffsetYMultiplier;
            orbitalOffsetZMultiplier = from.orbitalOffsetZMultiplier;

            radial = new XResourceMinMaxCurve(from.radial);
            radialMultiplier = from.radialMultiplier;

            speedModifier = new XResourceMinMaxCurve(from.speedModifier);
            speedModifierMultiplier = from.speedModifierMultiplier;
        }

        public void SetTo(ParticleSystem.VelocityOverLifetimeModule to)
        {
            to.enabled = enabled;
            to.x = x.ToUnity();
            to.y = y.ToUnity();
            to.z = z.ToUnity();
            to.xMultiplier = xMultiplier;
            to.yMultiplier = yMultiplier;
            to.zMultiplier = zMultiplier;

            to.space = space;
            to.orbitalX = orbitalX.ToUnity();
            to.orbitalY = orbitalY.ToUnity();
            to.orbitalZ = orbitalZ.ToUnity();
            to.orbitalXMultiplier = orbitalXMultiplier;
            to.orbitalYMultiplier = orbitalYMultiplier;
            to.orbitalZMultiplier = orbitalZMultiplier;

            to.orbitalOffsetX = orbitalOffsetX.ToUnity();
            to.orbitalOffsetY = orbitalOffsetY.ToUnity();
            to.orbitalOffsetZ = orbitalOffsetZ.ToUnity();
            to.orbitalOffsetXMultiplier = orbitalOffsetXMultiplier;
            to.orbitalOffsetYMultiplier = orbitalOffsetYMultiplier;
            to.orbitalOffsetZMultiplier = orbitalOffsetZMultiplier;

            to.radial = radial.ToUnity();
            to.radialMultiplier = radialMultiplier;

            to.speedModifier = speedModifier.ToUnity();

            to.speedModifierMultiplier = speedModifierMultiplier;
        }
    }
}
