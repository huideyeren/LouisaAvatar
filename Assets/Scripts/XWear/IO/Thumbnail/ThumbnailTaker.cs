using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace XWear.IO.Thumbnail
{
    public static class ThumbnailUtil
    {
        public const int Width = 512;
        public const int Height = 512;

        public static Texture2D CreateThumbnailTexture(byte[] thumbnailBinary)
        {
            var result = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            result.LoadImage(thumbnailBinary);
            return result;
        }
    }

    public static class ThumbnailTaker
    {
        public static byte[] TakeThumbnail(GameObject target)
        {
            var smrs = target.GetComponentsInChildren<SkinnedMeshRenderer>();
            var minX = smrs.Select(x => x.bounds.min.x).Min();
            var maxX = smrs.Select(x => x.bounds.max.x).Max();
            var minY = smrs.Select(x => x.bounds.min.y).Min();
            var maxY = smrs.Select(x => x.bounds.max.y).Max();
            var maxZ = smrs.Select(x => x.bounds.max.z).Max();

            var centerX = (maxX + minX) / 2;
            var centerY = (maxY + minY) / 2;
            var cameraPosition = new Vector3(centerX, centerY, maxZ);

            var boundHeight = maxY - minY;

            var camera = CreateCameraObject(cameraPosition, ThumbnailUtil.Height, boundHeight);
            var cameraRenderTexture = CreateThumbnailRenderTexture();
            camera.targetTexture = cameraRenderTexture;

            RenderTexture.active = cameraRenderTexture;

            camera.Render();

            var result = GetThumbnailBinary(cameraRenderTexture);

            camera.targetTexture = null;
            RenderTexture.active = null;

            Object.DestroyImmediate(cameraRenderTexture);
            Object.DestroyImmediate(camera.gameObject);

            return result;
        }

        private static Camera CreateCameraObject(
            Vector3 position,
            int thumbnailHeight,
            float boundsHeight
        )
        {
            var cameraObject = new GameObject("ThumbnailCamera")
            {
                transform =
                {
                    position = position,
                    rotation = Quaternion.Euler(0, -180, 0),
                    localScale = Vector3.one
                }
            };

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.nearClipPlane = 0.01f;
            camera.orthographicSize = boundsHeight * thumbnailHeight / thumbnailHeight / 2;
            return camera;
        }

        private static RenderTexture CreateThumbnailRenderTexture()
        {
            return new RenderTexture(
                ThumbnailUtil.Width,
                ThumbnailUtil.Height,
                0,
                RenderTextureFormat.ARGB32
            );
        }

        private static byte[] GetThumbnailBinary(RenderTexture cameraRenderTexture)
        {
            var tmpTexture2d = new Texture2D(
                cameraRenderTexture.width,
                cameraRenderTexture.height,
                TextureFormat.RGBA32,
                false
            );

            tmpTexture2d.ReadPixels(
                new Rect(0, 0, cameraRenderTexture.width, cameraRenderTexture.height),
                0,
                0
            );

            var result = tmpTexture2d.EncodeToPNG();

            Object.DestroyImmediate(tmpTexture2d);

            return result;
        }
    }
}
