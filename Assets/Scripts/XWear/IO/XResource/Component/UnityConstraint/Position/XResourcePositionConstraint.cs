using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Serialization;
using XWear.IO.XResource.Component.ComponentPlugin;
using XWear.IO.XResource.Transform;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.Component.UnityConstraint.Position
{
    [Serializable]
    public class XResourcePositionConstraint : IXResourceComponent
    {
        public ComponentType ComponentType => ComponentType.UnityConstraint;
        public bool constraintActive;
        public float weight;

        public bool locked;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 translationAtRest;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 translationOffset;

        public Axis translationAxis;
        public int sourceCount;
        public List<ConstraintSource> constraintSources;

        public UnityEngine.Component AttachTo(GameObject attachTarget)
        {
            return null;
        }

        public XResourcePositionConstraint() { }

        public XResourcePositionConstraint(
            PositionConstraint constraint,
            GameObjectWithTransformCollector gameObjectCollector
        )
        {
            constraintActive = constraint.constraintActive;
            weight = constraint.weight;

            locked = constraint.locked;
            translationAtRest = constraint.translationAtRest;
            translationOffset = constraint.translationOffset;
            translationAxis = constraint.translationAxis;

            sourceCount = constraint.sourceCount;
            constraintSources = new List<ConstraintSource>();
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
            }
        }

        public void SetTo(
            PositionConstraint constraint,
            GameObjectWithTransformBuilder gameObjectBuilder
        )
        {
            constraint.constraintActive = constraintActive;
            constraint.weight = weight;

            constraint.locked = locked;
            constraint.translationAtRest = translationAtRest;
            constraint.translationOffset = translationOffset;
            constraint.translationAxis = translationAxis;

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
            }
        }
    }
}
