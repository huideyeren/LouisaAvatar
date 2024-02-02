using System;
using UnityEngine;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Component.Particle
{
    [Serializable]
    public class SubEmitterModule
    {
        public bool enabled;

        public SubEmitterModule() { }

        public SubEmitterModule(
            ParticleSystem.SubEmittersModule from,
            GameObjectWithTransformCollector gameObjectCollector
        )
        {
            enabled = from.enabled;
            if (enabled)
            {
                UnityEngine.Debug.LogWarning("SubEmitters is not supported");
            }
        }

        public void SetTo(ParticleSystem.SubEmittersModule to)
        {
            to.enabled = false;
            if (enabled)
            {
                UnityEngine.Debug.LogWarning("SubEmitters is not supported");
            }
        }
    }
}
