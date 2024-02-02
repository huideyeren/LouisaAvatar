using System;
using UnityEngine;

namespace XWear.IO.XResource.Component.Particle
{
    [Serializable]
    public class CollisionModule
    {
        public bool enabled;
        public ParticleSystemCollisionType type;
        public ParticleSystemCollisionMode mode;
        public XResourceMinMaxCurve dampen;
        public float dampenMultiplier;
        public XResourceMinMaxCurve bounce;
        public float bounceMultiplier;
        public XResourceMinMaxCurve lifetimeLoss;
        public float lifetimeLossMultiplier;
        public float minKillSpeed;
        public float maxKillSpeed;
        public LayerMask collidesWith;
        public ParticleSystemCollisionQuality quality;
        public float radiusScale;
        public int maxCollisionShapes;
        public bool enableDynamicColliders;
        public float colliderForce;
        public bool multiplyColliderForceByCollisionAngle;
        public bool multiplyColliderForceByParticleSpeed;
        public bool multiplyColliderForceByParticleSize;
        public bool sendCollisionMessages;

        public CollisionModule() { }

        public CollisionModule(ParticleSystem.CollisionModule from)
        {
            enabled = from.enabled;
            type = from.type;
            if (from.type == ParticleSystemCollisionType.Planes)
            {
                UnityEngine.Debug.LogWarning("Plane Collision is not supported");
            }

            mode = from.mode;
            dampen = new XResourceMinMaxCurve(from.dampen);
            dampenMultiplier = from.dampenMultiplier;
            bounce = new XResourceMinMaxCurve(from.bounce);
            bounceMultiplier = from.bounceMultiplier;
            lifetimeLoss = new XResourceMinMaxCurve(from.lifetimeLoss);
            lifetimeLossMultiplier = from.lifetimeLossMultiplier;
            minKillSpeed = from.minKillSpeed;
            maxKillSpeed = from.maxKillSpeed;
            radiusScale = from.radiusScale;
            quality = from.quality;
            collidesWith = from.collidesWith;
            maxCollisionShapes = from.maxCollisionShapes;
            enableDynamicColliders = from.enableDynamicColliders;
            colliderForce = from.colliderForce;
            multiplyColliderForceByCollisionAngle = from.multiplyColliderForceByCollisionAngle;
            multiplyColliderForceByParticleSpeed = from.multiplyColliderForceByParticleSpeed;
            multiplyColliderForceByParticleSize = from.multiplyColliderForceByParticleSize;
            sendCollisionMessages = from.sendCollisionMessages;
        }

        public void SetTo(ParticleSystem.CollisionModule to)
        {
            to.enabled = enabled;
            to.type = type;
            if (type == ParticleSystemCollisionType.Planes)
            {
                UnityEngine.Debug.LogWarning("Plane Collision is not supported");
            }

            to.mode = mode;
            to.dampen = dampen.ToUnity();
            to.dampenMultiplier = dampenMultiplier;
            to.bounce = bounce.ToUnity();
            to.bounceMultiplier = bounceMultiplier;
            to.lifetimeLoss = lifetimeLoss.ToUnity();
            to.lifetimeLossMultiplier = lifetimeLossMultiplier;
            to.minKillSpeed = minKillSpeed;
            to.maxKillSpeed = maxKillSpeed;
            to.radiusScale = radiusScale;
            to.quality = quality;
            to.collidesWith = collidesWith;
            to.maxCollisionShapes = maxCollisionShapes;
            to.enableDynamicColliders = enableDynamicColliders;
            to.colliderForce = colliderForce;
            to.multiplyColliderForceByCollisionAngle = multiplyColliderForceByCollisionAngle;
            to.multiplyColliderForceByParticleSpeed = multiplyColliderForceByParticleSpeed;
            to.multiplyColliderForceByParticleSize = multiplyColliderForceByParticleSize;
            to.sendCollisionMessages = sendCollisionMessages;
        }
    }
}
