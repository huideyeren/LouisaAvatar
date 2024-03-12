using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Animations;
using XWear.IO.XResource.Component.ComponentPlugin;
using XWear.IO.XResource.Transform;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.Component.UnityConstraint.Aim
{
    [Serializable]
    public class XResourceAimConstraint : IXResourceComponent
    {
        public ComponentType ComponentType => ComponentType.UnityConstraint;
        public bool constraintActive;
        public float weight;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 aimVector;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 upVector;

        public AimConstraint.WorldUpType worldUpType;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 worldUpVector;

        public string worldUpObjectGuid;
        public bool locked;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 rotationAtRest;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 rotationOffset;

        public Axis rotationAxis;
        public int sourceCount;
        public List<ConstraintSource> constraintSources;

        public UnityEngine.Component AttachTo(GameObject attachTarget)
        {
            return null;
        }

        public XResourceAimConstraint() { }

        public XResourceAimConstraint(
            AimConstraint constraint,
            GameObjectWithTransformCollector gameObjectCollector
        )
        {
            constraintActive = constraint.constraintActive;
            weight = constraint.weight;
            aimVector = constraint.aimVector;
            upVector = constraint.upVector;
            worldUpType = constraint.worldUpType;
            worldUpVector = constraint.worldUpVector;
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
            rotationAxis = constraint.rotationAxis;
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
            AimConstraint aimConstraint,
            GameObjectWithTransformBuilder gameObjectBuilder
        )
        {
            aimConstraint.constraintActive = constraintActive;
            aimConstraint.weight = weight;
            aimConstraint.aimVector = aimVector;
            aimConstraint.upVector = upVector;
            aimConstraint.worldUpType = worldUpType;
            aimConstraint.worldUpVector = worldUpVector;

            if (!string.IsNullOrEmpty(worldUpObjectGuid))
            {
                var worldUpObject = BuildComponentUtil.GetBuildTransformRefFromGuid(
                    gameObjectBuilder,
                    worldUpObjectGuid
                );
                aimConstraint.worldUpObject = worldUpObject;
            }

            aimConstraint.locked = locked;
            aimConstraint.rotationAtRest = rotationAtRest;
            aimConstraint.rotationOffset = rotationOffset;
            aimConstraint.rotationAxis = rotationAxis;

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
                aimConstraint.AddSource(constraintSource);
            }
        }
    }
}
