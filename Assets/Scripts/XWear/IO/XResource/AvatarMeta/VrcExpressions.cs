using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.Serialization;

namespace XWear.IO.XResource.AvatarMeta
{
    [Serializable]
    public abstract class VrcAssetResource
    {
        public enum AssetType
        {
            AnimatorController,
            AnimationClip,
            AvatarMask,
            ExpressionsMainMenu,
            ExpressionsSubMenu,
            ExpressionsIcon,
            ExpressionParameters,
            BlendTree,
        }

        public AssetType type;
        public string Name;
        public string Guid;
        public abstract VrcAssetResource Copy();
    }

    [Serializable]
    public class VrcExpressionsMenuResource : VrcAssetResource
    {
        [Serializable]
        public class Control
        {
            public string iconGuid;
            public string subMenuGuid;
        }

        public List<Control> controls = new List<Control>();

        public override VrcAssetResource Copy()
        {
            var result = new VrcExpressionsMenuResource
            {
                Guid = Guid,
                Name = Name,
                type = type,
                // controlも複製
                controls = controls.ToList()
            };

            return result;
        }
    }

    [Serializable]
    public class SimpleVrcAssetResource : VrcAssetResource
    {
        public override VrcAssetResource Copy()
        {
            return new SimpleVrcAssetResource()
            {
                Guid = Guid,
                Name = Name,
                type = type
            };
        }
    }

    [Serializable]
    public class VrcExpressions
    {
        public string expressionMenuGuid;
        public string expressionParametersGuid;
    }
}
