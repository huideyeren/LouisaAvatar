using System;
using UnityEngine;

namespace XWear.IO.XResource.Component.Particle
{
    [Serializable]
    public class XResourceBurst
    {
        public float time;
        public XResourceMinMaxCurve count;
        public int cycleCount;
        public float repeatInterval;
        public float probability;

        public XResourceBurst() { }

        public XResourceBurst(ParticleSystem.Burst from)
        {
            time = from.time;
            count = new XResourceMinMaxCurve(from.count);
            cycleCount = from.cycleCount;
            repeatInterval = from.repeatInterval;
            probability = from.probability;
        }

        public ParticleSystem.Burst ToUnity()
        {
            return new ParticleSystem.Burst()
            {
                time = time,
                count = count.ToUnity(),
                cycleCount = cycleCount,
                repeatInterval = repeatInterval,
                probability = probability,
            };
        }
    }
}
