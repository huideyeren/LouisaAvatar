namespace XWear.IO
{
    public static class XWearIOUtil
    {
        public static readonly Newtonsoft.Json.JsonSerializerSettings SerializerSetting =
            new Newtonsoft.Json.JsonSerializerSettings()
            {
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                MaxDepth = 128
            };
    }
}
