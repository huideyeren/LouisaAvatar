using System;

namespace XWear.IO.XResource.Texture
{
    [Serializable]
    public class TextureImportSettings
    {
        public bool streamingMipmaps;
        public bool mipmapEnabled;
        public bool crunchedCompression;
        public int compressionQuality;
        public int maxTextureSize = 2048;
    }
}
