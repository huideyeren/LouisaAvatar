using System.Collections.Generic;
using XWear.IO.XResource.Util;

namespace XWear.IO.XResource.Material
{
    public static class MaterialTagUtil
    {
        public enum MaterialTagKey
        {
            Queue,
            RenderType,
            DisableBatching,
            ForceNoShadowCasting,
            IgnoreProjector,
            CanUseSpriteAtlas,
            PreviewTyp,
        }

        private static readonly Dictionary<MaterialTagKey, string> TagKeyStringValueMap =
            new Dictionary<MaterialTagKey, string>()
            {
                { MaterialTagKey.Queue, "Queue" },
                { MaterialTagKey.RenderType, "RenderType" },
                { MaterialTagKey.DisableBatching, "DisableBatching" },
                { MaterialTagKey.ForceNoShadowCasting, "ForceNoShadowCasting" },
                { MaterialTagKey.IgnoreProjector, "IgnoreProjector" },
                { MaterialTagKey.CanUseSpriteAtlas, "CanUseSpriteAtlas" },
                { MaterialTagKey.PreviewTyp, "PreviewType" },
            };

        public static string GetTarKeyStringValue(MaterialTagKey tagKey)
        {
            return TagKeyStringValueMap[tagKey];
        }

        public static List<MaterialTag> CollectMaterialTags(UnityEngine.Material sourceMaterial)
        {
            var result = new List<MaterialTag>();
            var tagKeys = TagKeyStringValueMap.FlipKvp();
            foreach (var tagKeyKvp in tagKeys)
            {
                var value = sourceMaterial.GetTag(tagKeyKvp.Key, false);
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                result.Add(
                    new MaterialTag() { MaterialTagKey = tagKeyKvp.Value, TagValue = value }
                );
            }

            return result;
        }

        public static void BuildMaterialTags(
            List<MaterialTag> materialTags,
            UnityEngine.Material target
        )
        {
            foreach (var materialTag in materialTags)
            {
                if (
                    TagKeyStringValueMap.TryGetValue(
                        materialTag.MaterialTagKey,
                        out var tagKeyStringValue
                    )
                )
                {
                    target.SetOverrideTag(tagKeyStringValue, materialTag.TagValue);
                }
            }
        }
    }
}
