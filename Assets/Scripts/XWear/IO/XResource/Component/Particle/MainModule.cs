using System;
using UnityEngine;

namespace XWear.IO.XResource.Component.Particle
{
    [Serializable]
    public class MainModule
    {
        public float duration;
        public bool loop;
        public bool prewarm;

        public XResourceMinMaxCurve startDelay;
        public XResourceMinMaxCurve startLifetime;
        public XResourceMinMaxCurve startSpeed;
        public float startDelayMultiplier;
        public float startLifetimeMultiplier;
        public float startSpeedMultiplier;

        public bool startSize3D;
        public XResourceMinMaxCurve startSize;
        public float startSizeMultiplier;

        public bool startRotation3D;
        public XResourceMinMaxCurve startRotation;
        public float startRotationMultiplier;

        public float flipRotation;
        public XResourceMinMaxGradient startColor;

        public XResourceMinMaxCurve gravityModifier;
        public float gravityModifierMultiplier;

        public ParticleSystemSimulationSpace simulationSpace;
        public float simulationSpeed;
        public bool useUnscaledTime;
        public ParticleSystemScalingMode scalingMode;
        public bool playOnAwake;
        public ParticleSystemEmitterVelocityMode emitterVelocityMode;
        public int maxParticles;
        public ParticleSystemStopAction stopAction;
        public ParticleSystemCullingMode cullingMode;
        public ParticleSystemRingBufferMode ringBufferMode;

        public MainModule() { }

        public MainModule(ParticleSystem.MainModule mainModule)
        {
            duration = mainModule.duration;
            loop = mainModule.loop;
            prewarm = mainModule.prewarm;
            startDelay = new XResourceMinMaxCurve(mainModule.startDelay);
            startLifetime = new XResourceMinMaxCurve(mainModule.startLifetime);
            startSpeed = new XResourceMinMaxCurve(mainModule.startSpeed);
            startDelayMultiplier = mainModule.startDelayMultiplier;
            startLifetimeMultiplier = mainModule.startLifetimeMultiplier;
            startSpeedMultiplier = mainModule.startSpeedMultiplier;

            startSize3D = mainModule.startSize3D;
            startSize = new XResourceMinMaxCurve(mainModule.startSize);
            startSizeMultiplier = mainModule.startSizeMultiplier;

            startRotation3D = mainModule.startRotation3D;
            startRotation = new XResourceMinMaxCurve(mainModule.startRotation);
            startRotationMultiplier = mainModule.startRotationMultiplier;

            flipRotation = mainModule.flipRotation;
            startColor = new XResourceMinMaxGradient(mainModule.startColor);

            gravityModifier = new XResourceMinMaxCurve(mainModule.gravityModifier);
            gravityModifierMultiplier = mainModule.gravityModifierMultiplier;

            simulationSpace = mainModule.simulationSpace;
            simulationSpeed = mainModule.simulationSpeed;
            useUnscaledTime = mainModule.useUnscaledTime;
            scalingMode = mainModule.scalingMode;
            playOnAwake = mainModule.playOnAwake;
            emitterVelocityMode = mainModule.emitterVelocityMode;
            maxParticles = mainModule.maxParticles;
            stopAction = mainModule.stopAction;
            cullingMode = mainModule.cullingMode;
            ringBufferMode = mainModule.ringBufferMode;
        }

        public void SetTo(ParticleSystem.MainModule to)
        {
            to.duration = duration;
            to.loop = loop;
            to.prewarm = prewarm;
            to.startDelay = startDelay.ToUnity();
            to.startLifetime = startLifetime.ToUnity();
            to.startSpeed = startSpeed.ToUnity();
            to.startDelayMultiplier = startDelayMultiplier;
            to.startLifetimeMultiplier = startLifetimeMultiplier;
            to.startSpeedMultiplier = startSpeedMultiplier;
            to.startSize3D = startSize3D;
            to.startSize = startSize.ToUnity();
            to.startSizeMultiplier = startSizeMultiplier;
            to.startRotation3D = startRotation3D;
            to.startRotation = startRotation.ToUnity();
            to.startRotationMultiplier = startRotationMultiplier;
            to.flipRotation = flipRotation;
            to.startColor = startColor.ToUnity();
            to.gravityModifier = gravityModifier.ToUnity();
            to.gravityModifierMultiplier = gravityModifierMultiplier;
            to.simulationSpace = simulationSpace;
            to.simulationSpeed = simulationSpeed;
            to.useUnscaledTime = useUnscaledTime;
            to.scalingMode = scalingMode;
            to.playOnAwake = playOnAwake;
            to.emitterVelocityMode = emitterVelocityMode;
            to.maxParticles = maxParticles;
            to.stopAction = stopAction;
            to.cullingMode = cullingMode;
            to.ringBufferMode = ringBufferMode;
        }
    }
}
