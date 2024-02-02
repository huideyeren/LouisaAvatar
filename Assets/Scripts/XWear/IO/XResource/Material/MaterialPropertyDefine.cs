using UnityEngine;

namespace XWear.IO.XResource.Material
{
    // いる？
    public class MaterialPropertyDefine : ScriptableObject
    {
        public string shaderName;

        public void GenerateFromShader(UnityEngine.Shader shader)
        {
            shaderName = shader.name;
        }
    }
}
