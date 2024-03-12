using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Animations;
using XWear.IO.XResource.Component.ComponentPlugin;
using XWear.IO.XResource.Transform;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.Component.UnityConstraint.Scale
{
    [Serializable]
    public class XResourceScaleConstraint : IXResourceComponent
    {
        public ComponentType ComponentType => ComponentType.UnityConstraint;
        public bool constraintActive;
        public float weight;

        public bool locked;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 scaleAtRest;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 scaleOffset;

        public Axis scalingAxis;
        public int sourceCount;
        public List<ConstraintSource> constraintSources;

        public UnityEngine.Component AttachTo(GameObject attachTarget)
        {
            return null;
        }

        public XResourceScaleConstraint() { }

        public XResourceScaleConstraint(
            ScaleConstraint constraint,
            GameObjectWithTransformCollector gameObjectCollector
        )
        {
            constraintActive = constraint.constraintActive;
            weight = constraint.weight;

            locked = constraint.locked;
            scaleAtRest = constraint.scaleAtRest;
            scaleOffset = constraint.scaleOffset;
            scalingAxis = constraint.scalingAxis;

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
            ScaleConstraint constraint,
            GameObjectWithTransformBuilder gameObjectBuilder
        )
        {
            constraint.constraintActive = constraintActive;
            constraint.weight = weight;

            constraint.locked = locked;
            constraint.scaleAtRest = scaleAtRest;
            constraint.scaleOffset = scaleOffset;
            constraint.scalingAxis = scalingAxis;

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
