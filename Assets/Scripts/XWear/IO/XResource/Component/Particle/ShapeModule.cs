using System;
using Newtonsoft.Json;
using UnityEngine;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Texture;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.Component.Particle
{
    [Serializable]
    public class ShapeModule
    {
        public bool enabled;
        public ParticleSystemShapeType shapeType;
        public float radius;
        public float radiusThickness;
        public float arc;
        public XResourceMinMaxCurve arcSpeed;
        public float arcSpeedMultiplier;
        public XResourceTexture texture = null;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 position;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 rotation;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 scale;

        public bool alignToDirection;
        public float randomDirectionAmount;
        public float sphericalDirectionAmount;
        public float randomPositionAmount;

        public ShapeModule() { }

        public ShapeModule(
            ParticleSystem.ShapeModule from,
            XResourceContainerUtil.XResourceArchiver archiver
        )
        {
            enabled = from.enabled;
            shapeType = from.shapeType;
            radius = from.radius;
            radiusThickness = from.radiusThickness;
            arc = from.arc;
            arcSpeed = new XResourceMinMaxCurve(from.arcSpeed);
            arcSpeedMultiplier = from.arcSpeedMultiplier;

            if (from.texture != null)
            {
                // todo Fix
                var shapeTexture = new XResourceTexture(
                    guid: Guid.NewGuid().ToString(),
                    sourceTexture: from.texture
                );
                /*archiver.AddXResourceTexture(shapeTexture, from.texture);
                texture = shapeTexture;*/
            }

            position = from.position;
            rotation = from.rotation;
            scale = from.scale;
            alignToDirection = from.alignToDirection;
            randomDirectionAmount = from.randomDirectionAmount;
            sphericalDirectionAmount = from.sphericalDirectionAmount;
            randomPositionAmount = from.randomPositionAmount;
        }

        public static void CopyUnityComponent(
            ParticleSystem.ShapeModule from,
            ParticleSystem.ShapeModule to
        )
        {
            to.enabled = from.enabled;
            to.shapeType = from.shapeType;
            to.radius = from.radius;
            to.radiusThickness = from.radiusThickness;
            to.arc = from.arc;
            to.arcSpeed = new XResourceMinMaxCurve(from.arcSpeed).ToUnity();
            to.arcSpeedMultiplier = from.arcSpeedMultiplier;
            to.texture = from.texture;

            to.position = from.position;
            to.rotation = from.rotation;
            to.scale = from.scale;
            to.alignToDirection = from.alignToDirection;
            to.randomDirectionAmount = from.randomDirectionAmount;
            to.sphericalDirectionAmount = from.sphericalDirectionAmount;
            to.randomPositionAmount = from.randomPositionAmount;
        }

        public void SetTo(
            ParticleSystem.ShapeModule to,
            XResourceContainerUtil.XResourceOpener opener
        )
        {
            to.enabled = enabled;
            to.shapeType = shapeType;
            to.radius = radius;
            to.radiusThickness = radiusThickness;
            to.arc = arc;
            to.arcSpeed = arcSpeed.ToUnity();
            to.arcSpeedMultiplier = arcSpeedMultiplier;

            if (texture != null)
            {
                to.texture = (Texture2D)opener.GetXResourceTextureAndCreate(texture);
            }

            to.position = position;
            to.rotation = rotation;
            to.scale = scale;
            to.alignToDirection = alignToDirection;
            to.randomDirectionAmount = randomDirectionAmount;
            to.sphericalDirectionAmount = sphericalDirectionAmount;
            to.randomPositionAmount = randomPositionAmount;
        }
    }
}
