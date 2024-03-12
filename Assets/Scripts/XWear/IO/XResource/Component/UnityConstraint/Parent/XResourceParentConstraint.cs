using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Animations;
using XWear.IO.XResource.Component.ComponentPlugin;
using XWear.IO.XResource.Transform;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.Component.UnityConstraint.Parent
{
    [Serializable]
    public class ConstraintSourceOffset
    {
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 translationOffset;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 rotationOffset;
    }

    [Serializable]
    public class XResourceParentConstraint : IXResourceComponent
    {
        public ComponentType ComponentType => ComponentType.UnityConstraint;
        public bool constraintActive;
        public float weight;
        public bool locked;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 translationAtRest;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 rotationAtRest;

        public Axis translationAxis;
        public Axis rotationAxis;

        public int sourceCount;
        public List<ConstraintSource> constraintSources;
        public List<ConstraintSourceOffset> constraintSourceOffsets;

        public UnityEngine.Component AttachTo(GameObject attachTarget)
        {
            return null;
        }

        public XResourceParentConstraint() { }

        public XResourceParentConstraint(
            ParentConstraint constraint,
            GameObjectWithTransformCollector gameObjectCollector
        )
        {
            constraintActive = constraint.constraintActive;
            weight = constraint.weight;

            locked = constraint.locked;
            translationAtRest = constraint.translationAtRest;
            rotationAtRest = constraint.rotationAtRest;

            translationAxis = constraint.translationAxis;
            rotationAxis = constraint.rotationAxis;

            sourceCount = constraint.sourceCount;
            constraintSources = new List<ConstraintSource>();
            constraintSourceOffsets = new List<ConstraintSourceOffset>();
            for (int i = 0; i < sourceCount; i++)
            {
                var source = constraint.GetSource(i);
                var newConstraintSource = new ConstraintSource();
                if (source.sourceTransform != null)
                {
                    var sourceTransformGuid = CollectComponentUtil.GetCollectTransformRefGuid(
                        gameObjectCollector,
                        source.sourceTransform
                    );
                    newConstraintSource.transformGuid = sourceTransformGuid;
                }

                newConstraintSource.weight = source.weight;

                constraintSources.Add(newConstraintSource);

                var translationOffset = constraint.GetTranslationOffset(i);
                var rotationOffset = constraint.GetRotationOffset(i);
                var newConstraintOffset = new ConstraintSourceOffset
                {
                    translationOffset = translationOffset,
                    rotationOffset = rotationOffset
                };
                constraintSourceOffsets.Add(newConstraintOffset);
            }
        }

        public void SetTo(
            ParentConstraint constraint,
            GameObjectWithTransformBuilder gameObjectBuilder
        )
        {
            constraint.constraintActive = constraintActive;
            constraint.weight = weight;

            constraint.locked = locked;
            constraint.translationAtRest = translationAtRest;
            constraint.rotationAtRest = rotationAtRest;
            constraint.translationAxis = translationAxis;
            constraint.rotationAxis = rotationAxis;

            for (int i = 0; i < sourceCount; i++)
            {
                var constraintSource = new UnityEngine.Animations.ConstraintSource();
                var sourceGuid = constraintSources[i].transformGuid;
                var sourceWeight = constraintSources[i].weight;
                if (!string.IsNullOrEmpty(sourceGuid))
                {
                    constraintSource.sourceTransform =
                        BuildComponentUtil.GetBuildTransformRefFromGuid(
                            gameObjectBuilder,
                            sourceGuid
                        );
                }

                constraintSource.weight = sourceWeight;
                constraint.AddSource(constraintSource);

                constraint.SetTranslationOffset(i, constraintSourceOffsets[i].translationOffset);
                constraint.SetRotationOffset(i, constraintSourceOffsets[i].rotationOffset);
            }
        }
    }
}
