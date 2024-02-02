using System;
using System.Collections.Generic;
using UnityEngine;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Texture.Util;

namespace XWear.IO.XResource.Texture
{
    public abstract class TextureBuilderBase
    {
        /// <summary>
        /// XResourceTextureのGuidからTextureの実体を引くためのマップ
        /// </summary>
        protected readonly Dictionary<string, XResourceTextureInstance> GuidToTexture =
            new Dictionary<string, XResourceTextureInstance>();

        public readonly List<XResourceTextureInstance> TextureInstances =
            new List<XResourceTextureInstance>();

        /// <summary>
        ///
        /// </summary>
        /// <param name="xItem"></param>
        /// <param name="opener"></param>
        public abstract List<XResourceTextureInstance> BuildXResourceTextures(
            XItem xItem,
            XResourceContainerUtil.XResourceOpener opener
        );

        /// <summary>
        /// XResourceTextureのGuidから構築されたTextureの実体を取得する
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="texture"></param>
        public bool TryGetTextureFromGuid(string guid, out UnityEngine.Texture texture)
        {
            if (GuidToTexture.TryGetValue(guid, out var textureInstance))
            {
                texture = textureInstance.TextureInstance;
                return true;
            }

            throw new Exception($"Texture:{guid} is not found");
        }
    }
}
