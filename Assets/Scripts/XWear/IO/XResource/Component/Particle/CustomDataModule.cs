using System;
using UnityEngine;

namespace XWear.IO.XResource.Component.Particle
{
    [Serializable]
    public class CustomDataModule
    {
        public bool enabled;

        public CustomDataModule() { }

        public CustomDataModule(ParticleSystem.CustomDataModule from)
        {
            enabled = from.enabled;
            if (from.enabled)
            {
                UnityEngine.Debug.LogWarning("CustomDataModule is not supported");
            }
        }

        public void SetTo(ParticleSystem.CustomDataModule to)
        {
            to.enabled = false;

            if (enabled)
            {
                UnityEngine.Debug.LogWarning("CustomDataModule is not supported");
            }
        }
    }
}
