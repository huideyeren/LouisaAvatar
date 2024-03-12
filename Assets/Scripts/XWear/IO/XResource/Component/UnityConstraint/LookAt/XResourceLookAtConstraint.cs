using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Animations;
using XWear.IO.XResource.Component.ComponentPlugin;
using XWear.IO.XResource.Transform;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.Component.UnityConstraint.LookAt
{
    [Serializable]
    public class XResourceLookAtConstraint : IXResourceComponent
    {
        public ComponentType ComponentType => ComponentType.UnityConstraint;
        public bool constraintActive;
        public float weight;
        public bool useUpObject;
        public float roll;
        public string worldUpObjectGuid;
        public bool locked;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 rotationAtRest;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 rotationOffset;

        public int sourceCount;
        public List<ConstraintSource> constraintSources;

        public UnityEngine.Component AttachTo(GameObject attachTarget)
        {
            return null;
        }

        public XResourceLookAtConstraint() { }

        public XResourceLookAtConstraint(
            LookAtConstraint constraint,
            GameObjectWithTransformCollector gameObjectCollector
        )
        {
            constraintActive = constraint.constraintActive;
            weight = constraint.weight;
            useUpObject = constraint.useUpObject;
            roll = constraint.roll;

            if (constraint.worldUpObject != null)
            {
                worldUpObjectGuid = CollectComponentUtil.GetCollectTransformRefGuid(
                    gameObjectCollector,
                    constraint.worldUpObject
                );
            }

            locked = constraint.locked;
            rotationAtRest = constraint.rotationAtRest;
            rotationOffset = constraint.rotationOffset;
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
            LookAtConstraint constraint,
            GameObjectWithTransformBuilder gameObjectBuilder
        )
        {
            constraint.constraintActive = constraintActive;
            constraint.weight = weight;
            constraint.useUpObject = useUpObject;
            constraint.roll = roll;

            if (!string.IsNullOrEmpty(worldUpObjectGuid))
            {
                var worldUpObject = BuildComponentUtil.GetBuildTransformRefFromGuid(
                    gameObjectBuilder,
                    worldUpObjectGuid
                );
                constraint.worldUpObject = worldUpObject;
            }

            constraint.locked = locked;
            constraint.rotationAtRest = rotationAtRest;
            constraint.rotationOffset = rotationOffset;

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
