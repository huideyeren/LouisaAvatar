using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XWear.IO.XResource.Util.UnityGenericJsonUtil
{
    public class Vector4Converter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var m = (UnityEngine.Vector4)value;

            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(m.x);

            writer.WritePropertyName("y");
            writer.WriteValue(m.y);

            writer.WritePropertyName("z");
            writer.WriteValue(m.z);

            writer.WritePropertyName("w");
            writer.WriteValue(m.w);

            writer.WriteEnd();
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer
        )
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return new UnityEngine.Vector4();
            }

            var obj = JObject.Load(reader);
            return new UnityEngine.Vector4()
            {
                x = (float)obj["x"],
                y = (float)obj["y"],
                z = (float)obj["z"],
                w = (float)obj["w"],
            };
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(UnityEngine.Vector4);
        }
    }
}
