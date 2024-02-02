using System;
using UnityEngine;

namespace XWear.IO.XResource.Component.Particle
{
    [Serializable]
    public class ColorOverLifetimeModule
    {
        public bool enabled;
        public XResourceMinMaxGradient color;

        public ColorOverLifetimeModule() { }

        public ColorOverLifetimeModule(ParticleSystem.ColorOverLifetimeModule from)
        {
            enabled = from.enabled;
            color = new XResourceMinMaxGradient(from.color);
        }

        public void SetTo(ParticleSystem.ColorOverLifetimeModule to)
        {
            to.enabled = enabled;
            to.color = color.ToUnity();
        }
    }
}
