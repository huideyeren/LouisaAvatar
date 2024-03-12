using System;
using XWear.IO.XResource.Texture.Util;

namespace XWear.IO.XResource.Texture
{
    [Serializable]
    public class XResourceTexture
    {
        public string Guid = "";

        public TextureParam TextureParam;
        public TextureImportSettings TextureImportSettings;

        // todo マイグレーションによってここの2項目をTextureImportSettingsに入れるべきである
        public bool isNormal;
        public bool alphaIsTransparency;

        public XResourceTexture(string guid, UnityEngine.Texture sourceTexture)
        {
            Guid = guid;
            if (sourceTexture is UnityEngine.Texture2D texture2d)
            {
                TextureParam = new Texture2DParam(texture2d);
            }
            else if (sourceTexture is UnityEngine.RenderTexture renderTexture)
            {
                TextureParam = new RenderTextureParam(renderTexture);
            }

            TextureImportSettings = new TextureImportSettings();
        }

        public XResourceTexture() { }
    }

    public class XResourceTextureInstance
    {
        public string Guid;
        public UnityEngine.Texture TextureInstance;
        public XResourceTexture XResourceTexture;
    }
}
