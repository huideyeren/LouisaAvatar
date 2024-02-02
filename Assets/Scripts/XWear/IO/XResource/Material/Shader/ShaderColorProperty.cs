using Newtonsoft.Json;
using UnityEngine;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.Material.Shader
{
    public class ShaderColorProperty : IXResourceShaderProperty
    {
        [JsonConverter(typeof(ColorConverter))]
        public Color Color = Color.white;

        [field: SerializeField]
        public string PropertyName { get; set; } = "";

        public ShaderColorProperty(string propertyName, Color color)
        {
            PropertyName = propertyName;
            Color = color;
        }

        public ShaderColorProperty() { }

        public IXResourceShaderProperty Copy()
        {
            return new ShaderColorProperty() { Color = Color, PropertyName = PropertyName };
        }
    }
}
