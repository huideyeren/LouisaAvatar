using UnityEngine;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Component.Particle;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Component.ComponentPlugin
{
    public class ParticleBuildComponentPlugin : DefaultBuildComponentPluginBase<XResourceParticle>
    {
        // UnityCollider,Lightよりもあとに実行される必要がある
        public override int Order => 11;

        public override UnityEngine.Component BuildAndAttach(
            GameObjectWithTransformBuilder gameObjectBuilder,
            SkinnedMeshRendererDataBuilder skinnedMeshRendererBuilder,
            MaterialBuilderBase materialBuilder,
            GameObject attachTarget,
            XResourceContainerUtil.XResourceOpener opener,
            AssetSaver assetSaver
        )
        {
            var xResource = Context.Get();
            var particle = attachTarget.AddComponent<ParticleSystem>();
            var particleRenderer = attachTarget.GetComponent<ParticleSystemRenderer>();
            xResource.mainModule.SetTo(particle.main);
            xResource.emissionModule.SetTo(particle.emission);
            xResource.shapeModule.SetTo(particle.shape, opener);
            xResource.velocityOverLifetime.SetTo(particle.velocityOverLifetime);
            xResource.limitVelocityOverLifetime.SetTo(particle.limitVelocityOverLifetime);
            xResource.inheritVelocity.SetTo(particle.inheritVelocity);
            xResource.forceOverLifetime.SetTo(particle.forceOverLifetime);
            xResource.colorOverLifetime.SetTo(particle.colorOverLifetime);
            xResource.colorBySpeed.SetTo(particle.colorBySpeed);
            xResource.sizeOverLifetime.SetTo(particle.sizeOverLifetime);
            xResource.sizeBySpeed.SetTo(particle.sizeBySpeed);
            xResource.rotationOverLifetime.SetTo(particle.rotationOverLifetime);
            xResource.rotationBySpeed.SetTo(particle.rotationBySpeed);
            xResource.externalForces.SetTo(particle.externalForces, particle, opener);
            xResource.noise.SetTo(particle.noise);
            xResource.collision.SetTo(particle.collision);
            xResource.trigger.SetTo(particle.trigger, gameObjectBuilder);
            xResource.subEmitters.SetTo(particle.subEmitters);
            xResource.textureSheetAnimation.SetTo(particle.textureSheetAnimation);
            xResource.lights.SetTo(particle.lights, gameObjectBuilder);
            xResource.trails.SetTo(particle.trails);
            xResource.customData.SetTo(particle.customData);

            xResource.renderer.SetTo(particleRenderer, materialBuilder);

            return particle;
        }
    }
}
