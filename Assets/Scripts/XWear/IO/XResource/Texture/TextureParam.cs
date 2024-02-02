using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace XWear.IO.XResource.Texture
{
    [Serializable]
    public class TextureParam
    {
        public enum TextureType
        {
            RenderTexture,
            Texture2d
        }

        public string name;
        public int width;
        public int height;
        public int anisoLevel;
        public TextureWrapMode wrapMode;
        public TextureWrapMode wrapModeU;
        public TextureWrapMode wrapModeV;
        public TextureWrapMode wrapModeW;
        public int mipMapCount;
        public GraphicsFormat graphicsFormat;
        public TextureDimension dimension;
        public bool linear;
        public TextureType textureType;
        public int depth;

        public TextureParam(UnityEngine.Texture texture)
        {
            name = texture.name;
            width = texture.width;
            height = texture.height;
            anisoLevel = texture.anisoLevel;
            wrapMode = texture.wrapMode;
            wrapModeU = texture.wrapModeU;
            wrapModeV = texture.wrapModeV;
            wrapModeW = texture.wrapModeW;
            mipMapCount = texture.mipmapCount;
            graphicsFormat = texture.graphicsFormat;
            dimension = texture.dimension;
            linear = !GraphicsFormatUtility.IsSRGBFormat(texture.graphicsFormat);
            switch (texture)
            {
                case RenderTexture rt:
                    textureType = TextureType.RenderTexture;
                    depth = rt.depth;
                    break;
                case Texture2D _:
                    textureType = TextureType.Texture2d;
                    break;
            }
        }

        public TextureParam() { }
    }

    [Serializable]
    public sealed class Texture2DParam : TextureParam
    {
        public TextureFormat textureFormat;

        public Texture2DParam(Texture2D texture2D)
            : base(texture2D)
        {
            textureFormat = texture2D.format;
        }

        public Texture2DParam() { }
    }

    [Serializable]
    public sealed class RenderTextureParam : TextureParam
    {
        public RenderTextureParam(RenderTexture renderTexture)
            : base(renderTexture) { }

        public RenderTextureParam() { }
    }
}
