using System;
using UnityEngine;

namespace XWear.IO.XResource.Component.Particle
{
    [Serializable]
    public class EmissionModule
    {
        public bool enabled;
        public XResourceMinMaxCurve rateOverTime;
        public float rateOverTimeMultiplier;

        public XResourceMinMaxCurve rateOverDistance;
        public float rateOverDistanceMultiplier;

        public int burstCount;
        public XResourceBurst[] bursts = Array.Empty<XResourceBurst>();

        public EmissionModule() { }

        public EmissionModule(ParticleSystem.EmissionModule emissionModule)
        {
            enabled = emissionModule.enabled;

            rateOverTime = new XResourceMinMaxCurve(emissionModule.rateOverTime);
            rateOverTimeMultiplier = emissionModule.rateOverTimeMultiplier;

            rateOverDistance = new XResourceMinMaxCurve(emissionModule.rateOverDistance);
            rateOverDistanceMultiplier = emissionModule.rateOverDistanceMultiplier;

            burstCount = emissionModule.burstCount;
            bursts = new XResourceBurst[burstCount];
            for (int i = 0; i < burstCount; i++)
            {
                bursts[i] = new XResourceBurst(emissionModule.GetBurst(i));
            }
        }

        public void SetTo(ParticleSystem.EmissionModule to)
        {
            to.enabled = enabled;
            to.rateOverTime = rateOverTime.ToUnity();
            to.rateOverTimeMultiplier = rateOverTimeMultiplier;
            to.rateOverDistance = rateOverDistance.ToUnity();
            to.rateOverDistanceMultiplier = rateOverDistanceMultiplier;
            to.burstCount = burstCount;
            for (int i = 0; i < burstCount; i++)
            {
                to.SetBurst(i, bursts[i].ToUnity());
            }
        }
    }
}
