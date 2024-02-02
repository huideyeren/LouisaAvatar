using UnityEngine;

namespace XWear.IO.XResource.Material.Shader
{
    public class ShaderTextureProperty : IXResourceShaderProperty
    {
        [field: SerializeField]
        public string PropertyName { get; set; } = "";
        public string TextureGuid = "";

        public ShaderTextureProperty(string propertyName, string textureGuid)
        {
            PropertyName = propertyName;
            TextureGuid = textureGuid;
        }

        public ShaderTextureProperty() { }

        public IXResourceShaderProperty Copy()
        {
            return new ShaderTextureProperty()
            {
                TextureGuid = TextureGuid,
                PropertyName = PropertyName
            };
        }
    }
}
