using UnityEngine;

namespace XWear.IO.XResource.Material.Shader
{
    public class ShaderFloatProperty : IXResourceShaderProperty
    {
        public float Value = 0.0f;

        [field: SerializeField]
        public string PropertyName { get; set; } = "";

        public ShaderFloatProperty(string propertyName, float value)
        {
            PropertyName = propertyName;
            Value = value;
        }

        public ShaderFloatProperty() { }

        public IXResourceShaderProperty Copy()
        {
            return new ShaderFloatProperty() { Value = Value, PropertyName = PropertyName };
        }
    }
}
