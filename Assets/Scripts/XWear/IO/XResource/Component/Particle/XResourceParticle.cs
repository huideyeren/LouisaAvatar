using System;
using UnityEngine;

namespace XWear.IO.XResource.Component.Particle
{
    [Serializable]
    public class XResourceParticle : IXResourceComponent
    {
        public ComponentType ComponentType => ComponentType.Particle;
        public MainModule mainModule;
        public EmissionModule emissionModule;
        public ShapeModule shapeModule;
        public VelocityOverLiefTimeModule velocityOverLifetime;
        public LimitVelocityOverLifetimeModule limitVelocityOverLifetime;
        public InheritVelocityModule inheritVelocity;
        public ForceOverLifetimeModule forceOverLifetime;
        public ColorOverLifetimeModule colorOverLifetime;
        public ColorBySpeedModule colorBySpeed;
        public SizeOverLifetimeModule sizeOverLifetime;
        public SizeBySpeedModule sizeBySpeed;
        public RotationOverLifetimeModule rotationOverLifetime;
        public RotationBySpeedModule rotationBySpeed;
        public ExternalForcesModule externalForces;
        public NoiseModule noise;
        public CollisionModule collision;
        public TriggerModule trigger;
        public SubEmitterModule subEmitters;
        public TextureSheetAnimationModule textureSheetAnimation;
        public LightsModule lights;
        public TrailModule trails;
        public CustomDataModule customData;
        public XResourceParticleRenderer renderer;

        public UnityEngine.Component AttachTo(GameObject attachTarget)
        {
            return null;
        }
    }
}
