namespace XWear.IO.XResource.Material.Shader
{
    public interface IXResourceShaderProperty
    {
        string PropertyName { get; set; }
        IXResourceShaderProperty Copy();
    }
}
