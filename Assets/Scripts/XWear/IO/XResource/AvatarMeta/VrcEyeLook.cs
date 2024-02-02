using System;
using Newtonsoft.Json;
using UnityEngine;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.AvatarMeta
{
    [Serializable]
    public class VrcEyeLook
    {
        [Serializable]
        public class EyelidRotationSet
        {
            public EyeRotationSet upper = new EyeRotationSet();
            public EyeRotationSet lower = new EyeRotationSet();
        }

        [Serializable]
        public class EyeRotationSet
        {
            public bool linked;

            [JsonConverter(typeof(QuaternionConverter))]
            public Quaternion left;

            [JsonConverter(typeof(QuaternionConverter))]
            public Quaternion right;
        }

        public enum EyelidType
        {
            None,
            Bones,
            BlendShapes,
        }

        public bool enable;
        public float eyeMovementsConfidence;
        public float eyeMovementsExcitement;
        public string leftEyeGuid;
        public string rightEyeGuid;
        public EyeRotationSet lookingStraight = new EyeRotationSet();
        public EyeRotationSet lookingUp = new EyeRotationSet();
        public EyeRotationSet lookingDown = new EyeRotationSet();
        public EyeRotationSet lookingLeft = new EyeRotationSet();
        public EyeRotationSet lookingRight = new EyeRotationSet();

        public EyelidType eyelidType;
        public string eyelidSkinnedMeshRendererGuid;
        public int[] eyelidBlendShapes = Array.Empty<int>();

        public string upperLeftEyelidGuid;
        public string upperRightEyelidGuid;
        public string lowerLeftEyelidGuid;
        public string lowerRightEyelidGuid;

        public EyelidRotationSet eyelidsDefault = new EyelidRotationSet();
        public EyelidRotationSet eyelidsClosed = new EyelidRotationSet();
        public EyelidRotationSet eyelidsLookingUp = new EyelidRotationSet();
        public EyelidRotationSet eyelidsLookingDown = new EyelidRotationSet();
    }
}
