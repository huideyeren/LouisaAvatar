using Newtonsoft.Json;
using UnityEngine;
using XWear.IO.XResource.Util.UnityGenericJsonUtil;

namespace XWear.IO.XResource.Material.Shader
{
    public class ShaderVectorProperty : IXResourceShaderProperty
    {
        [JsonConverter(typeof(Vector4Converter))]
        public Vector4 Vector;

        [field: SerializeField]
        public string PropertyName { get; set; } = "";

        public ShaderVectorProperty(string propertyName, Vector4 vector)
        {
            PropertyName = propertyName;
            Vector = vector;
        }

        public ShaderVectorProperty() { }

        public IXResourceShaderProperty Copy()
        {
            return new ShaderVectorProperty() { Vector = Vector, PropertyName = PropertyName };
        }
    }
}
