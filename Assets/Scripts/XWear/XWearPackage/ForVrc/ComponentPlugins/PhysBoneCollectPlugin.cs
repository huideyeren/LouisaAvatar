using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;
using XWear.IO.XResource;
using XWear.IO.XResource.Animation;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Component;
using XWear.IO.XResource.Component.ComponentPlugin;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.SwingParameter;
using XWear.IO.XResource.SwingParameter.PhysBone;
using XWear.IO.XResource.Transform;

namespace XWear.XWearPackage.ForVrc.ComponentPlugins
{
    public class PhysBoneCollectPlugin : DefaultCollectComponentPluginBase<VRCPhysBone>
    {
        // Colliderを先に集めておきたいので、この数値はColliderCollectPluginのOrderより大きい必要がある
        public override int Order => 11;

        protected override void CopyComponent(Transform attachTarget, VRCPhysBone sourceComponent)
        {
            var resultVrcPhysBone = attachTarget.gameObject.AddComponent<VRCPhysBone>();

            resultVrcPhysBone.version = sourceComponent.version;

            resultVrcPhysBone.rootTransform = sourceComponent.rootTransform;
            foreach (var ignoreTransform in sourceComponent.ignoreTransforms)
            {
                resultVrcPhysBone.ignoreTransforms.Add(ignoreTransform);
            }

            resultVrcPhysBone.endpointPosition = sourceComponent.endpointPosition;
            resultVrcPhysBone.multiChildType = sourceComponent.multiChildType;
            resultVrcPhysBone.integrationType = sourceComponent.integrationType;

            #region Force

            resultVrcPhysBone.pull = sourceComponent.pull;
            resultVrcPhysBone.pullCurve = sourceComponent.pullCurve;

            resultVrcPhysBone.spring = sourceComponent.spring;
            resultVrcPhysBone.springCurve = sourceComponent.springCurve;

            resultVrcPhysBone.gravity = sourceComponent.gravity;
            resultVrcPhysBone.gravityCurve = sourceComponent.gravityCurve;

            resultVrcPhysBone.gravityFalloff = sourceComponent.gravityFalloff;
            resultVrcPhysBone.gravityFalloffCurve = sourceComponent.gravityFalloffCurve;

            resultVrcPhysBone.stiffness = sourceComponent.stiffness;
            resultVrcPhysBone.stiffnessCurve = sourceComponent.stiffnessCurve;

            resultVrcPhysBone.immobileType = sourceComponent.immobileType;
            resultVrcPhysBone.immobile = sourceComponent.immobile;
            resultVrcPhysBone.immobileCurve = sourceComponent.immobileCurve;

            #endregion

            #region Limits

            resultVrcPhysBone.limitType = sourceComponent.limitType;

            resultVrcPhysBone.maxAngleX = sourceComponent.maxAngleX;
            resultVrcPhysBone.maxAngleXCurve = sourceComponent.maxAngleXCurve;

            resultVrcPhysBone.maxAngleZ = sourceComponent.maxAngleZ;
            resultVrcPhysBone.maxAngleZCurve = sourceComponent.maxAngleZCurve;

            resultVrcPhysBone.limitRotation = sourceComponent.limitRotation;
            resultVrcPhysBone.limitRotationXCurve = sourceComponent.limitRotationXCurve;
            resultVrcPhysBone.limitRotationYCurve = sourceComponent.limitRotationYCurve;
            resultVrcPhysBone.limitRotationZCurve = sourceComponent.limitRotationZCurve;

            #endregion

            #region Collision

            resultVrcPhysBone.radius = sourceComponent.radius;
            resultVrcPhysBone.radiusCurve = sourceComponent.radiusCurve;
            resultVrcPhysBone.allowCollision = sourceComponent.allowCollision;
            foreach (var collider in sourceComponent.colliders)
            {
                resultVrcPhysBone.colliders.Add(collider);
            }

            #endregion

            #region Stretch,Squish

            resultVrcPhysBone.stretchMotion = sourceComponent.stretchMotion;
            resultVrcPhysBone.stretchMotionCurve = sourceComponent.stretchMotionCurve;

            resultVrcPhysBone.maxStretch = sourceComponent.maxStretch;
            resultVrcPhysBone.maxStretchCurve = sourceComponent.maxStretchCurve;

            resultVrcPhysBone.maxSquish = sourceComponent.maxSquish;
            resultVrcPhysBone.maxSquishCurve = sourceComponent.maxSquishCurve;

            #endregion

            #region Grab,Pose

            resultVrcPhysBone.allowGrabbing = sourceComponent.allowGrabbing;
            resultVrcPhysBone.allowPosing = sourceComponent.allowPosing;
            resultVrcPhysBone.grabMovement = sourceComponent.grabMovement;
            resultVrcPhysBone.snapToHand = sourceComponent.snapToHand;

            #endregion

            #region Options

            resultVrcPhysBone.parameter = sourceComponent.parameter;
            resultVrcPhysBone.isAnimated = sourceComponent.isAnimated;
            resultVrcPhysBone.resetWhenDisabled = sourceComponent.resetWhenDisabled;

            #endregion
        }

        public override IXResourceComponent Collect(
            XItem xItem,
            MaterialCollectorBase materialCollector,
            GameObjectWithTransformCollector gameObjectCollector,
            SkinnedMeshRendererDataCollector skinnedMeshRendererCollector,
            XResourceContainerUtil.XResourceArchiver archiver
        )
        {
            var vrcPhysBone = Context.Get();
            var param = new PhysBoneParameter();
            param.physBoneVersion = (int)vrcPhysBone.version;

            param.rootTransformGuid =
                CollectUtilForVrcProject.GetCollectedTransformGuidWithCheckNull(
                    vrcPhysBone.rootTransform,
                    gameObjectCollector
                );
            param.ignoreTransforms = new string[vrcPhysBone.ignoreTransforms.Count];
            for (var index = 0; index < vrcPhysBone.ignoreTransforms.Count; index++)
            {
                var ignoreTransform = vrcPhysBone.ignoreTransforms[index];
                param.ignoreTransforms[index] =
                    CollectUtilForVrcProject.GetCollectedTransformGuidWithCheckNull(
                        ignoreTransform,
                        gameObjectCollector
                    );
            }

            param.endpointPosition = vrcPhysBone.endpointPosition;
            param.multiChildType = (int)vrcPhysBone.multiChildType;
            param.integrationType = (int)vrcPhysBone.integrationType;

            #region Force

            param.pull = vrcPhysBone.pull;
            param.pullCurve = new XResourceAnimationCurve(vrcPhysBone.pullCurve);

            param.spring = vrcPhysBone.spring;
            param.springCurve = new XResourceAnimationCurve(vrcPhysBone.springCurve);

            param.gravity = vrcPhysBone.gravity;
            param.gravityCurve = new XResourceAnimationCurve(vrcPhysBone.gravityCurve);

            param.gravityFallOff = vrcPhysBone.gravityFalloff;
            param.gravityFallOffCurve = new XResourceAnimationCurve(
                vrcPhysBone.gravityFalloffCurve
            );

            param.stiffness = vrcPhysBone.stiffness;
            param.stiffnessCurve = new XResourceAnimationCurve(vrcPhysBone.stiffnessCurve);

            param.immobileType = (int)vrcPhysBone.immobileType;
            param.immobile = vrcPhysBone.immobile;
            param.immobileCurve = new XResourceAnimationCurve(vrcPhysBone.immobileCurve);

            #endregion

            #region Limits

            param.limitType = (int)vrcPhysBone.limitType;

            param.maxAngleX = vrcPhysBone.maxAngleX;
            param.maxAngleXCurve = new XResourceAnimationCurve(vrcPhysBone.maxAngleXCurve);

            param.maxAngleZ = vrcPhysBone.maxAngleZ;
            param.maxAngleZCurve = new XResourceAnimationCurve(vrcPhysBone.maxAngleZCurve);

            param.limitRotation = vrcPhysBone.limitRotation;
            param.limitRotationXCurve = new XResourceAnimationCurve(
                vrcPhysBone.limitRotationXCurve
            );
            param.limitRotationYCurve = new XResourceAnimationCurve(
                vrcPhysBone.limitRotationYCurve
            );
            param.limitRotationZCurve = new XResourceAnimationCurve(
                vrcPhysBone.limitRotationZCurve
            );

            #endregion

            #region Collision

            param.radius = vrcPhysBone.radius;
            param.radiusCurve = new XResourceAnimationCurve(vrcPhysBone.radiusCurve);
            param.allowCollision = (int)vrcPhysBone.allowCollision;
            param.colliderTransforms = new string[vrcPhysBone.colliders.Count];
            for (var index = 0; index < vrcPhysBone.colliders.Count; index++)
            {
                var collider = vrcPhysBone.colliders[index];
                if (collider == null)
                {
                    continue;
                }
                param.colliderTransforms[index] =
                    CollectUtilForVrcProject.GetCollectedTransformGuidWithCheckNull(
                        collider.transform,
                        gameObjectCollector
                    );
            }
            #endregion

            #region Stretch,Squish

            param.stretchMotion = vrcPhysBone.stretchMotion;
            param.stretchMotionCurve = new XResourceAnimationCurve(vrcPhysBone.stretchMotionCurve);

            param.maxStretch = vrcPhysBone.maxStretch;
            param.maxStretchCurve = new XResourceAnimationCurve(vrcPhysBone.maxStretchCurve);

            param.maxSquish = vrcPhysBone.maxSquish;
            param.maxSquishCurve = new XResourceAnimationCurve(vrcPhysBone.maxSquishCurve);

            #endregion

            #region Grab,Pose

            param.allowGrabbing = (int)vrcPhysBone.allowGrabbing;
            param.allowPosing = (int)vrcPhysBone.allowPosing;
            param.grabMoment = vrcPhysBone.grabMovement;
            param.snapToHand = vrcPhysBone.snapToHand;

            #endregion

            #region Options

            param.parameter = vrcPhysBone.parameter;
            param.isAnimated = vrcPhysBone.isAnimated;
            param.resetWhenDisabled = vrcPhysBone.resetWhenDisabled;

            #endregion


            return new XResourcePhysBoneComponent(param);
        }
    }
}
