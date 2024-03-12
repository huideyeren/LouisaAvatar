using System;
using System.Collections.Generic;
using System.Linq;
using XWear.IO.XResource.Archive;

namespace XWear.IO.XResource.Texture
{
    public abstract class TextureCollectorBase
    {
        public readonly Dictionary<
            UnityEngine.Texture,
            XResourceTexture
        > TextureToXResourceTextureMemo = new Dictionary<UnityEngine.Texture, XResourceTexture>();

        public readonly List<XResourceTexture> CurrentContextReferenced =
            new List<XResourceTexture>();

        public List<XResourceTexture> GetCollectedXResourceTexture()
        {
            return TextureToXResourceTextureMemo.Select(x => x.Value).ToList();
        }

        /// <summary>
        /// テクスチャの実体からXResourceTextureを生成する。
        /// 生成した結果は_textureToXResourceTextureMemoに保存される
        /// すでにsourceTextureを元に生成したXResourceTextureが存在する場合は、
        /// _textureToXResourceTextureMemoからその結果を返す。
        /// </summary>
        /// <param name="sourceTexture"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        protected abstract XResourceTexture TryCollectAndAdd(UnityEngine.Texture sourceTexture);

        public XResourceTexture TryCollect(UnityEngine.Texture sourceTexture)
        {
            var xResourceTexture = TryCollectAndAdd(sourceTexture);
            if (!CurrentContextReferenced.Contains(xResourceTexture))
            {
                CurrentContextReferenced.Add(xResourceTexture);
            }

            return xResourceTexture;
        }

        public void Flush()
        {
            CurrentContextReferenced.Clear();
        }

        public XResourceTexture TryGetXResourceTextureFromTextureInstance(
            UnityEngine.Texture texture
        )
        {
            if (TextureToXResourceTextureMemo.TryGetValue(texture, out var value))
            {
                return value;
            }

            throw new Exception($"Texture Not found: {texture.name}");
        }

        public abstract void Archive(XResourceContainerUtil.XResourceArchiver archiver);
    }
}
