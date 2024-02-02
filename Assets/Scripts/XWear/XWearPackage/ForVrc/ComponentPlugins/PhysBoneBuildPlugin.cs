using System;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;
using XWear.IO;
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
    public class PhysBoneBuildPlugin : DefaultBuildComponentPluginBase<XResourcePhysBoneComponent>
    {
        // Colliderを先に構築する必要があるので、この数値はColliderBuildPluginのOrderより大きい必要がある
        public override int Order => 11;

        public override Component BuildAndAttach(
            GameObjectWithTransformBuilder gameObjectBuilder,
            SkinnedMeshRendererDataBuilder skinnedMeshRendererBuilder,
            MaterialBuilderBase materialBuilder,
            GameObject attachTarget,
            XResourceContainerUtil.XResourceOpener opener,
            AssetSaver assetSaver
        )
        {
            var vrcPhysBone = attachTarget.AddComponent<VRCPhysBone>();
            var param = Context.Get().PhysBoneParam;
            vrcPhysBone.version = (VRCPhysBoneBase.Version)param.physBoneVersion;

            vrcPhysBone.rootTransform =
                BuildUtilForVrcProject.GetBuiltTransformWithCheckStringEmpty(
                    param.rootTransformGuid,
                    gameObjectBuilder
                );
            foreach (var ignoreTransform in param.ignoreTransforms)
            {
                vrcPhysBone.ignoreTransforms.Add(
                    BuildUtilForVrcProject.GetBuiltTransformWithCheckStringEmpty(
                        ignoreTransform,
                        gameObjectBuilder
                    )
                );
            }

            vrcPhysBone.endpointPosition = param.endpointPosition;
            vrcPhysBone.multiChildType = (VRCPhysBoneBase.MultiChildType)param.multiChildType;
            vrcPhysBone.integrationType = (VRCPhysBoneBase.IntegrationType)param.integrationType;

            #region Force

            vrcPhysBone.pull = param.pull;
            vrcPhysBone.pullCurve = TryConvertUnityAnimationCurve(param.pullCurve);

            vrcPhysBone.spring = param.spring;
            vrcPhysBone.springCurve = TryConvertUnityAnimationCurve(param.springCurve);

            vrcPhysBone.gravity = param.gravity;
            vrcPhysBone.gravityCurve = TryConvertUnityAnimationCurve(param.gravityCurve);

            vrcPhysBone.gravityFalloff = param.gravityFallOff;
            vrcPhysBone.gravityFalloffCurve = TryConvertUnityAnimationCurve(
                param.gravityFallOffCurve
            );

            vrcPhysBone.stiffness = param.stiffness;
            vrcPhysBone.stiffnessCurve = TryConvertUnityAnimationCurve(param.stiffnessCurve);

            vrcPhysBone.immobileType = (VRCPhysBoneBase.ImmobileType)param.immobileType;
            vrcPhysBone.immobile = param.immobile;
            vrcPhysBone.immobileCurve = TryConvertUnityAnimationCurve(param.immobileCurve);

            #endregion

            #region Limits

            vrcPhysBone.limitType = (VRCPhysBoneBase.LimitType)param.limitType;

            vrcPhysBone.maxAngleX = param.maxAngleX;
            vrcPhysBone.maxAngleXCurve = TryConvertUnityAnimationCurve(param.maxAngleXCurve);

            vrcPhysBone.maxAngleZ = param.maxAngleZ;
            vrcPhysBone.maxAngleZCurve = TryConvertUnityAnimationCurve(param.maxAngleZCurve);

            vrcPhysBone.limitRotation = param.limitRotation;
            vrcPhysBone.limitRotationXCurve = TryConvertUnityAnimationCurve(
                param.limitRotationXCurve
            );
            vrcPhysBone.limitRotationYCurve = TryConvertUnityAnimationCurve(
                param.limitRotationYCurve
            );
            vrcPhysBone.limitRotationZCurve = TryConvertUnityAnimationCurve(
                param.limitRotationZCurve
            );

            #endregion

            #region Collision

            vrcPhysBone.radius = param.radius;
            vrcPhysBone.radiusCurve = TryConvertUnityAnimationCurve(param.radiusCurve);

            vrcPhysBone.allowCollision = (VRCPhysBoneBase.AdvancedBool)param.allowCollision;
            foreach (var colliderTransform in param.colliderTransforms)
            {
                if (string.IsNullOrEmpty(colliderTransform))
                {
                    continue;
                }

                var physBoneCollider = BuildUtilForVrcProject
                    .GetBuiltTransformWithCheckStringEmpty(colliderTransform, gameObjectBuilder)
                    .GetComponent<VRCPhysBoneCollider>();
                if (physBoneCollider != null)
                {
                    vrcPhysBone.colliders.Add(physBoneCollider);
                }
            }

            #endregion

            #region Stretch,Squish

            vrcPhysBone.stretchMotion = param.stretchMotion;
            vrcPhysBone.stretchMotionCurve = TryConvertUnityAnimationCurve(
                param.stretchMotionCurve
            );

            vrcPhysBone.maxStretch = param.maxStretch;
            vrcPhysBone.maxStretchCurve = TryConvertUnityAnimationCurve(param.maxStretchCurve);

            vrcPhysBone.maxSquish = param.maxSquish;
            vrcPhysBone.maxSquishCurve = TryConvertUnityAnimationCurve(param.maxSquishCurve);

            #endregion

            #region Grab,Pose

            vrcPhysBone.allowGrabbing = (VRCPhysBoneBase.AdvancedBool)param.allowGrabbing;
            vrcPhysBone.allowPosing = (VRCPhysBoneBase.AdvancedBool)param.allowPosing;
            vrcPhysBone.grabMovement = param.grabMoment;
            vrcPhysBone.snapToHand = param.snapToHand;

            #endregion

            #region Options

            vrcPhysBone.parameter = param.parameter;
            vrcPhysBone.isAnimated = param.isAnimated;
            vrcPhysBone.resetWhenDisabled = param.resetWhenDisabled;

            #endregion

            return vrcPhysBone;
        }

        private AnimationCurve TryConvertUnityAnimationCurve(XResourceAnimationCurve curve)
        {
            if (curve == null)
            {
                return new AnimationCurve()
                {
                    keys = Array.Empty<Keyframe>(),
                    postWrapMode = WrapMode.ClampForever,
                    preWrapMode = WrapMode.ClampForever
                };
            }

            return curve.GetUnityAnimationCurve();
        }
    }
}
