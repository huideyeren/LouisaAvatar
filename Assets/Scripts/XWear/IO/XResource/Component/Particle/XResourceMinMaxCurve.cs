using System;
using UnityEngine;
using XWear.IO.XResource.Animation;

namespace XWear.IO.XResource.Component.Particle
{
    [Serializable]
    public class XResourceMinMaxCurve
    {
        public ParticleSystemCurveMode mode;
        public float curveMultiplier;
        public XResourceAnimationCurve curveMin;
        public XResourceAnimationCurve curveMax;
        public XResourceAnimationCurve curve;
        public float constant;
        public float constantMin;
        public float constantMax;

        public XResourceMinMaxCurve() { }

        public XResourceMinMaxCurve(ParticleSystem.MinMaxCurve from)
        {
            mode = from.mode;
            curveMultiplier = from.curveMultiplier;
            curve = new XResourceAnimationCurve(from.curve);
            curveMin = new XResourceAnimationCurve(from.curveMin);
            curveMax = new XResourceAnimationCurve(from.curveMax);
            constant = from.constant;
            constantMin = from.constantMin;
            constantMax = from.constantMax;
        }

        public ParticleSystem.MinMaxCurve ToUnity()
        {
            return new ParticleSystem.MinMaxCurve()
            {
                mode = mode,
                curve = curve.GetUnityAnimationCurve(),
                curveMultiplier = curveMultiplier,
                curveMin = curveMin.GetUnityAnimationCurve(),
                curveMax = curveMax.GetUnityAnimationCurve(),
                constant = constant,
                constantMin = constantMin,
                constantMax = constantMax,
            };
        }
    }
}
