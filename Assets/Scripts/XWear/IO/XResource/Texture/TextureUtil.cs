using System;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Object = UnityEngine.Object;

namespace XWear.IO.XResource.Texture
{
    public static class TextureUtil
    {
        public static byte[] GetTextureBytes(UnityEngine.Texture sourceTexture)
        {
            switch (sourceTexture)
            {
                case Texture2D texture2D:
                    return GetTexture2DBytes(texture2D);
                case RenderTexture renderTexture:
                    return GetRenderTextureBytes(renderTexture);
                case Cubemap:
                    // todo Cubemapは現在対応できていない
                    throw new NotImplementedException("Cube map texture is not supported");
                default:
                    throw new ArgumentOutOfRangeException($"{sourceTexture.GetType()}");
            }
        }

        private static byte[] GetTexture2DBytes(Texture2D texture2d)
        {
            var resultTexture2d = CloneTexture2D(texture2d);
            var result = resultTexture2d.EncodeToPNG();
            Object.DestroyImmediate(resultTexture2d);
            return result;
        }

        private static byte[] GetRenderTextureBytes(RenderTexture renderTexture)
        {
            var creationFlags =
                renderTexture.mipmapCount > 1
                    ? TextureCreationFlags.MipChain
                    : TextureCreationFlags.None;

            var tmpTexture2D = new Texture2D(
                renderTexture.width,
                renderTexture.height,
                format: renderTexture.graphicsFormat,
                mipCount: renderTexture.mipmapCount,
                flags: creationFlags
            );

            var tmpRenderTexture = RenderTexture.GetTemporary(
                width: renderTexture.width,
                height: renderTexture.height,
                depthBuffer: 0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Default
            );

            Graphics.Blit(renderTexture, tmpRenderTexture);
            RenderTexture.active = tmpRenderTexture;
            tmpTexture2D.ReadPixels(
                new Rect(0, 0, tmpRenderTexture.width, tmpRenderTexture.height),
                0,
                0
            );
            tmpTexture2D.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(tmpRenderTexture);
            var bytes = tmpTexture2D.EncodeToPNG();
            Object.DestroyImmediate(tmpTexture2D);
            return bytes;
        }

        public static UnityEngine.Texture CreateTexture(
            XResourceTexture xResourceTexture,
            byte[] textureBytes
        )
        {
            var textureParam = xResourceTexture.TextureParam;
            switch (textureParam)
            {
                case Texture2DParam texture2DParam:
                    return CreateTexture2D(texture2DParam, textureBytes);
                case RenderTextureParam renderTextureParam:
                    return CreateRenderTexture(renderTextureParam, textureBytes);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Texture2D CreateTexture2D(Texture2DParam texture2DParam, byte[] textureBytes)
        {
            var resultTexture2d = new Texture2D(
                texture2DParam.width,
                texture2DParam.height,
                texture2DParam.textureFormat,
                texture2DParam.mipMapCount > 0,
                texture2DParam.linear
            );

            resultTexture2d.LoadImage(textureBytes, markNonReadable: false);
            resultTexture2d.name = texture2DParam.name;

            return resultTexture2d;
        }

        private static RenderTexture CreateRenderTexture(
            RenderTextureParam renderTextureParam,
            byte[] textureBytes
        )
        {
            var tmpTexture2D = new Texture2D(2, 2);
            tmpTexture2D.LoadImage(textureBytes);
            var resultRenderTexture = new RenderTexture(
                width: renderTextureParam.width,
                height: renderTextureParam.height,
                depth: renderTextureParam.depth,
                format: renderTextureParam.graphicsFormat,
                mipCount: renderTextureParam.mipMapCount
            );

            Graphics.Blit(tmpTexture2D, resultRenderTexture);

            Object.DestroyImmediate(tmpTexture2D);

            resultRenderTexture.name = renderTextureParam.name;

            return resultRenderTexture;
        }

        public static Texture2D CopyTexture(
            this UnityEngine.Texture source,
            TextureFormat format,
            UnityEngine.Material material = null
        )
        {
            var tmp = RenderTexture.GetTemporary(
                source.width,
                source.height,
                depthBuffer: 0,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Linear
            );

            if (material != null)
            {
                Graphics.Blit(source, tmp, material);
            }
            else
            {
                Graphics.Blit(source, tmp);
            }

            var result = new Texture2D(
                source.width,
                source.height,
                format,
                source.mipmapCount > 1,
                true
            );

            result.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            result.Apply();

            RenderTexture.ReleaseTemporary(tmp);

            return result;
        }

        public static Texture2D CloneTexture2D(this Texture2D sourceTexture, bool linear = false)
        {
            var width = sourceTexture.width;
            var height = sourceTexture.height;

            var tmpRenderTexture = new RenderTexture(sourceTexture.width, sourceTexture.height, 0);

            Graphics.Blit(sourceTexture, tmpRenderTexture);

            var result = new Texture2D(
                width,
                height,
                TextureFormat.RGBA32,
                sourceTexture.mipmapCount > 1,
                linear
            );

            result.name = sourceTexture.name;
            result.anisoLevel = sourceTexture.anisoLevel;
            result.filterMode = sourceTexture.filterMode;
            result.mipMapBias = sourceTexture.mipMapBias;
            result.wrapMode = sourceTexture.wrapMode;
            result.wrapModeU = sourceTexture.wrapModeU;
            result.wrapModeV = sourceTexture.wrapModeV;
            result.wrapModeW = sourceTexture.wrapModeW;

            result.ReadPixels(new Rect(0, 0, sourceTexture.width, sourceTexture.height), 0, 0);
            result.Apply();

            RenderTexture.active = null;

            Object.DestroyImmediate(tmpRenderTexture);
            return result;
        }
    }
}
