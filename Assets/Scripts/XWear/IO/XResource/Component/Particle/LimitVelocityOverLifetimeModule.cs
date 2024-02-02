using System;
using UnityEngine;

namespace XWear.IO.XResource.Component.Particle
{
    [Serializable]
    public class LimitVelocityOverLifetimeModule
    {
        public bool enabled;
        public bool separateAxes;
        public XResourceMinMaxCurve limit;
        public XResourceMinMaxCurve limitX;
        public XResourceMinMaxCurve limitY;
        public XResourceMinMaxCurve limitZ;
        public float limitXMultiplier;
        public float limitYMultiplier;
        public float limitZMultiplier;
        public float dampen;
        public XResourceMinMaxCurve drag;
        public float dragMultiplier;
        public bool multiplyDragByParticleSize;
        public bool multiplyDragByParticleVelocity;

        public LimitVelocityOverLifetimeModule() { }

        public LimitVelocityOverLifetimeModule(ParticleSystem.LimitVelocityOverLifetimeModule from)
        {
            enabled = from.enabled;
            separateAxes = from.separateAxes;

            limit = new XResourceMinMaxCurve(from.limit);
            limitX = new XResourceMinMaxCurve(from.limitX);
            limitY = new XResourceMinMaxCurve(from.limitY);
            limitZ = new XResourceMinMaxCurve(from.limitZ);
            limitXMultiplier = from.limitXMultiplier;
            limitYMultiplier = from.limitYMultiplier;
            limitZMultiplier = from.limitZMultiplier;

            dampen = from.dampen;
            drag = new XResourceMinMaxCurve(from.drag);
            dragMultiplier = from.dragMultiplier;

            multiplyDragByParticleSize = from.multiplyDragByParticleSize;
            multiplyDragByParticleVelocity = from.multiplyDragByParticleVelocity;
        }

        public void SetTo(ParticleSystem.LimitVelocityOverLifetimeModule to)
        {
            to.enabled = enabled;
            to.separateAxes = separateAxes;

            to.limit = limit.ToUnity();
            to.limitX = limitX.ToUnity();
            to.limitY = limitY.ToUnity();
            to.limitZ = limitZ.ToUnity();
            to.limitXMultiplier = limitXMultiplier;
            to.limitYMultiplier = limitYMultiplier;
            to.limitZMultiplier = limitZMultiplier;

            to.dampen = dampen;
            to.drag = drag.ToUnity();
            to.dragMultiplier = dragMultiplier;

            to.multiplyDragByParticleSize = multiplyDragByParticleSize;
            to.multiplyDragByParticleVelocity = multiplyDragByParticleVelocity;
        }
    }
}
