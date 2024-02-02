using System;
using UnityEngine;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Component.Particle;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Texture;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Component.ComponentPlugin
{
    public class ParticleCollectComponentPlugin : DefaultCollectComponentPluginBase<ParticleSystem>
    {
        // UnityCollider,Lightよりもあとに実行される必要がある
        public override int Order => 11;

        protected override void CopyComponent(
            UnityEngine.Transform attachTarget,
            ParticleSystem sourceComponent
        )
        {
            var result = attachTarget.gameObject.AddComponent<ParticleSystem>();
            var resultRenderer = attachTarget.gameObject.GetComponent<ParticleSystemRenderer>();

            var main = sourceComponent.main;
            new MainModule(main).SetTo(result.main);

            var emission = sourceComponent.emission;
            new EmissionModule(emission).SetTo(result.emission);

            var shape = sourceComponent.shape;
            ShapeModule.CopyUnityComponent(from: shape, to: result.shape);

            var velocityOverLifetime = sourceComponent.velocityOverLifetime;
            new VelocityOverLiefTimeModule(velocityOverLifetime).SetTo(result.velocityOverLifetime);

            var limitVelocityOverLifetime = sourceComponent.limitVelocityOverLifetime;
            new LimitVelocityOverLifetimeModule(limitVelocityOverLifetime).SetTo(
                result.limitVelocityOverLifetime
            );

            var inheritVelocity = sourceComponent.inheritVelocity;
            new InheritVelocityModule(inheritVelocity).SetTo(result.inheritVelocity);

            var forceOverLifetime = sourceComponent.forceOverLifetime;
            new ForceOverLifetimeModule(forceOverLifetime).SetTo(result.forceOverLifetime);

            var colorOverLifetime = sourceComponent.colorOverLifetime;
            new ColorOverLifetimeModule(colorOverLifetime).SetTo(result.colorOverLifetime);

            var colorBySpeed = sourceComponent.colorBySpeed;
            new ColorBySpeedModule(colorBySpeed).SetTo(result.colorBySpeed);

            var sizeOverLifetime = sourceComponent.sizeOverLifetime;
            new SizeOverLifetimeModule(sizeOverLifetime).SetTo(result.sizeOverLifetime);

            var sizeBySpeed = sourceComponent.sizeBySpeed;
            new SizeBySpeedModule(sizeBySpeed).SetTo(result.sizeBySpeed);

            var rotationOverLifetime = sourceComponent.rotationOverLifetime;
            new RotationOverLifetimeModule(rotationOverLifetime).SetTo(result.rotationOverLifetime);

            var rotationBySpeed = sourceComponent.rotationBySpeed;
            new RotationBySpeedModule(rotationBySpeed).SetTo(result.rotationBySpeed);

            var externalForces = sourceComponent.externalForces;
            ExternalForcesModule.CopyUnityComponent(
                from: externalForces,
                to: result.externalForces
            );

            var noise = sourceComponent.noise;
            new NoiseModule(noise).SetTo(result.noise);

            var collision = sourceComponent.collision;
            new CollisionModule(collision).SetTo(result.collision);

            var trigger = sourceComponent.trigger;
            TriggerModule.CopyUnityComponent(from: trigger, to: result.trigger);

            /*var subEmitters = sourceComponent.subEmitters;
             SubEmitterModule.CopyUnityComponent();*/

            var textureSheetAnimation = sourceComponent.textureSheetAnimation;
            new TextureSheetAnimationModule(textureSheetAnimation).SetTo(
                result.textureSheetAnimation
            );

            var lights = sourceComponent.lights;
            LightsModule.CopyUnityComponent(from: lights, to: result.lights);

            var trail = sourceComponent.trails;
            new TrailModule(trail).SetTo(result.trails);

            var customData = sourceComponent.customData;
            new CustomDataModule(customData).SetTo(result.customData);

            var renderer = sourceComponent.GetComponent<ParticleSystemRenderer>();
            XResourceParticleRenderer.CopyUnityComponent(from: renderer, to: resultRenderer);
        }

        public override IXResourceComponent Collect(
            XItem xItem,
            MaterialCollectorBase materialCollector,
            GameObjectWithTransformCollector gameObjectCollector,
            SkinnedMeshRendererDataCollector skinnedMeshRendererCollector,
            XResourceContainerUtil.XResourceArchiver archiver
        )
        {
            var particle = Context.Get();
            var result = new XResourceParticle();
            var main = particle.main;
            result.mainModule = new MainModule(main);

            var emission = particle.emission;
            result.emissionModule = new EmissionModule(emission);

            var shape = particle.shape;
            result.shapeModule = new ShapeModule(shape, archiver);

            var velocityOverLifetime = particle.velocityOverLifetime;
            result.velocityOverLifetime = new VelocityOverLiefTimeModule(velocityOverLifetime);

            var limitVelocityOverLifetime = particle.limitVelocityOverLifetime;
            result.limitVelocityOverLifetime = new LimitVelocityOverLifetimeModule(
                limitVelocityOverLifetime
            );

            var inheritVelocity = particle.inheritVelocity;
            result.inheritVelocity = new InheritVelocityModule(inheritVelocity);

            var forceOverLifetime = particle.forceOverLifetime;
            result.forceOverLifetime = new ForceOverLifetimeModule(forceOverLifetime);

            var colorOverLifetime = particle.colorOverLifetime;
            result.colorOverLifetime = new ColorOverLifetimeModule(colorOverLifetime);

            var colorBySpeed = particle.colorBySpeed;
            result.colorBySpeed = new ColorBySpeedModule(colorBySpeed);

            var sizeOverLifetime = particle.sizeOverLifetime;
            result.sizeOverLifetime = new SizeOverLifetimeModule(sizeOverLifetime);

            var sizeBySpeed = particle.sizeBySpeed;
            result.sizeBySpeed = new SizeBySpeedModule(sizeBySpeed);

            var rotationOverLifetime = particle.rotationOverLifetime;
            result.rotationOverLifetime = new RotationOverLifetimeModule(rotationOverLifetime);

            var rotationBySpeed = particle.rotationBySpeed;
            result.rotationBySpeed = new RotationBySpeedModule(rotationBySpeed);

            var externalForces = particle.externalForces;
            result.externalForces = new ExternalForcesModule(externalForces, archiver);

            var noise = particle.noise;
            result.noise = new NoiseModule(noise);

            var collision = particle.collision;
            result.collision = new CollisionModule(collision);

            var trigger = particle.trigger;
            result.trigger = new TriggerModule(trigger, gameObjectCollector);

            var subEmitters = particle.subEmitters;
            result.subEmitters = new SubEmitterModule(subEmitters, gameObjectCollector);

            var textureSheetAnimation = particle.textureSheetAnimation;
            result.textureSheetAnimation = new TextureSheetAnimationModule(textureSheetAnimation);

            var lights = particle.lights;
            result.lights = new LightsModule(lights, gameObjectCollector);

            var trail = particle.trails;
            result.trails = new TrailModule(trail);

            var customData = particle.customData;
            result.customData = new CustomDataModule(customData);

            var renderer = particle.GetComponent<ParticleSystemRenderer>();
            result.renderer = new XResourceParticleRenderer(renderer, materialCollector);

            return result;
        }
    }
}
