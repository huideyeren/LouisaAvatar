using System;
using Newtonsoft.Json;
using UnityEngine;
using XWear.IO.XResource.Animation;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.SwingParameter.PhysBone
{
    [Serializable]
    public class PhysBoneParameter
    {
        public int physBoneVersion;

        #region Transform

        public string rootTransformGuid;
        public string[] ignoreTransforms;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 endpointPosition;

        public int multiChildType;
        public int integrationType;

        #endregion

        #region Force

        public float pull;

        [SerializeReference]
        public XResourceAnimationCurve pullCurve;

        public float stiffness;

        [SerializeReference]
        public XResourceAnimationCurve stiffnessCurve;

        public float spring;

        [SerializeReference]
        public XResourceAnimationCurve springCurve;

        public float gravity;

        [SerializeReference]
        public XResourceAnimationCurve gravityCurve;

        public float gravityFallOff;

        [SerializeReference]
        public XResourceAnimationCurve gravityFallOffCurve;

        public int immobileType;
        public float immobile;

        [SerializeReference]
        public XResourceAnimationCurve immobileCurve;

        #endregion

        #region Limit

        public int limitType;
        public float maxAngleX;

        [SerializeReference]
        public XResourceAnimationCurve maxAngleXCurve;

        public float maxAngleZ;

        [SerializeReference]
        public XResourceAnimationCurve maxAngleZCurve;

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 limitRotation;

        [SerializeReference]
        public XResourceAnimationCurve limitRotationXCurve;

        [SerializeReference]
        public XResourceAnimationCurve limitRotationYCurve;

        [SerializeReference]
        public XResourceAnimationCurve limitRotationZCurve;

        #endregion

        #region Collision

        public float radius;

        [SerializeReference]
        public XResourceAnimationCurve radiusCurve;
        public int allowCollision;
        public string[] colliderTransforms;

        #endregion

        #region Stretch,Squish

        public float stretchMotion;

        [SerializeReference]
        public XResourceAnimationCurve stretchMotionCurve;

        public float maxStretch;

        [SerializeReference]
        public XResourceAnimationCurve maxStretchCurve;

        public float maxSquish;

        [SerializeReference]
        public XResourceAnimationCurve maxSquishCurve;

        #endregion

        #region Grab,Pose

        public int allowGrabbing;
        public int allowPosing;
        public float grabMoment;
        public bool snapToHand;

        #endregion

        #region Options

        public string parameter;
        public bool isAnimated;
        public bool resetWhenDisabled;

        #endregion


        public PhysBoneParameter() { }

        public PhysBoneParameter(PhysBoneParameter source)
        {
            physBoneVersion = source.physBoneVersion;
            rootTransformGuid = source.rootTransformGuid;
            ignoreTransforms = source.ignoreTransforms;
            endpointPosition = source.endpointPosition;
            multiChildType = source.multiChildType;
            integrationType = source.integrationType;
            pull = source.pull;
            pullCurve = source.pullCurve?.DeepCopy();
            stiffness = source.stiffness;
            stiffnessCurve = source.stiffnessCurve?.DeepCopy();
            spring = source.spring;
            springCurve = source.springCurve?.DeepCopy();
            gravity = source.gravity;
            gravityCurve = source.gravityCurve?.DeepCopy();
            gravityFallOff = source.gravityFallOff;
            gravityFallOffCurve = source.gravityFallOffCurve?.DeepCopy();
            immobileType = source.immobileType;
            immobile = source.immobile;
            immobileCurve = source.immobileCurve?.DeepCopy();
            limitType = source.limitType;
            maxAngleX = source.maxAngleX;
            maxAngleXCurve = source.maxAngleXCurve?.DeepCopy();
            maxAngleZ = source.maxAngleZ;
            maxAngleZCurve = source.maxAngleZCurve?.DeepCopy();
            limitRotation = source.limitRotation;
            limitRotationXCurve = source.limitRotationXCurve?.DeepCopy();
            limitRotationYCurve = source.limitRotationYCurve?.DeepCopy();
            limitRotationZCurve = source.limitRotationZCurve?.DeepCopy();
            radius = source.radius;
            radiusCurve = source.radiusCurve?.DeepCopy();
            allowCollision = source.allowCollision;
            colliderTransforms = source.colliderTransforms;
            stretchMotion = source.stretchMotion;
            stretchMotionCurve = source.stretchMotionCurve?.DeepCopy();
            maxStretch = source.maxStretch;
            maxStretchCurve = source.maxStretchCurve?.DeepCopy();
            maxSquish = source.maxSquish;
            maxSquishCurve = source.maxSquishCurve?.DeepCopy();
            allowGrabbing = source.allowGrabbing;
            allowPosing = source.allowPosing;
            grabMoment = source.grabMoment;
            snapToHand = source.snapToHand;
            parameter = source.parameter;
            isAnimated = source.isAnimated;
            resetWhenDisabled = source.resetWhenDisabled;
        }
    }
}
