using System;
using Newtonsoft.Json;
using UnityEngine;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.Component.Particle
{
    [Serializable]
    public class XResourceParticleForceField
    {
        public ParticleSystemForceFieldShape shape;
        public float startRange;
        public float endRange;
        public XResourceMinMaxCurve directionX;
        public XResourceMinMaxCurve directionY;
        public XResourceMinMaxCurve directionZ;
        public XResourceMinMaxCurve gravity;
        public float gravityFocus;
        public XResourceMinMaxCurve rotationSpeed;
        public XResourceMinMaxCurve rotationAttraction;

        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 rotationRandomness;

        public XResourceMinMaxCurve drag;
        public bool multiplyDragByParticleSize;

        public bool multiplyDragByParticleVelocity;

        //public XResourceTexture vectorTexture = null;
        public XResourceMinMaxCurve vectorFieldSpeed;
        public XResourceMinMaxCurve vectorFieldAttraction;

        public XResourceParticleForceField() { }

        public XResourceParticleForceField(
            ParticleSystemForceField from,
            XResourceContainerUtil.XResourceArchiver archiver
        )
        {
            shape = from.shape;
            startRange = from.startRange;
            endRange = from.endRange;
            directionX = new XResourceMinMaxCurve(from.directionX);
            directionY = new XResourceMinMaxCurve(from.directionY);
            directionZ = new XResourceMinMaxCurve(from.directionZ);
            gravity = new XResourceMinMaxCurve(from.gravity);
            gravityFocus = from.gravityFocus;
            rotationSpeed = new XResourceMinMaxCurve(from.rotationSpeed);
            rotationAttraction = new XResourceMinMaxCurve(from.rotationAttraction);
            rotationRandomness = from.rotationRandomness;
            drag = new XResourceMinMaxCurve(from.drag);
            multiplyDragByParticleSize = from.multiplyDragByParticleSize;
            multiplyDragByParticleVelocity = from.multiplyDragByParticleVelocity;

            if (from.vectorField != null)
            {
                UnityEngine.Debug.LogWarning(
                    $"{typeof(ParticleSystemForceField)}.{nameof(from.vectorField)} is not supported"
                );
            }
            /*var vectorField = from.vectorField;
            if (vectorField != null)
            {
                var texture = new XResourceTexture(
                    guid: Guid.NewGuid().ToString(),
                    sourceTexture: from.vectorField
                );
                archiver.AddXResourceTexture(texture, vectorField);
            }*/

            vectorFieldSpeed = new XResourceMinMaxCurve(from.vectorFieldSpeed);
            vectorFieldAttraction = new XResourceMinMaxCurve(from.vectorFieldAttraction);
        }

        public ParticleSystemForceField AddParticleForceField(
            ParticleSystem particleSystem,
            XResourceContainerUtil.XResourceOpener opener
        )
        {
            var resultField = particleSystem.gameObject.AddComponent<ParticleSystemForceField>();

            resultField.shape = shape;
            resultField.startRange = startRange;
            resultField.endRange = endRange;
            resultField.directionX = directionX.ToUnity();
            resultField.directionY = directionY.ToUnity();
            resultField.directionZ = directionZ.ToUnity();
            resultField.gravity = gravity.ToUnity();
            resultField.gravityFocus = gravityFocus;
            resultField.rotationSpeed = rotationSpeed.ToUnity();
            resultField.rotationAttraction = rotationAttraction.ToUnity();
            resultField.rotationRandomness = rotationRandomness;
            resultField.drag = drag.ToUnity();
            resultField.multiplyDragByParticleSize = multiplyDragByParticleSize;
            resultField.multiplyDragByParticleVelocity = multiplyDragByParticleVelocity;

            /*if (vectorTexture != null)
            {
                var texture = (Texture3D)opener.GetXResourceTextureAndCreate(vectorTexture);
                resultField.vectorField = texture;
            }*/

            resultField.vectorFieldSpeed = vectorFieldSpeed.ToUnity();
            resultField.vectorFieldAttraction = vectorFieldAttraction.ToUnity();

            return resultField;
        }
    }
}
