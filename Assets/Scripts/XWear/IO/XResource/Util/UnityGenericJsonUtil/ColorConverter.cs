using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XWear.IO.XResource.Util.UnityGenericJsonUtil
{
    public class ColorConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var m = (UnityEngine.Color)value;

            writer.WriteStartObject();
            writer.WritePropertyName("r");
            writer.WriteValue(m.r);

            writer.WritePropertyName("g");
            writer.WriteValue(m.g);

            writer.WritePropertyName("b");
            writer.WriteValue(m.b);

            writer.WritePropertyName("a");
            writer.WriteValue(m.a);

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
                return new UnityEngine.Color();
            }

            var obj = JObject.Load(reader);
            return new UnityEngine.Color()
            {
                r = (float)obj["r"],
                g = (float)obj["g"],
                b = (float)obj["b"],
                a = (float)obj["a"],
            };
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(UnityEngine.Color);
        }
    }
}
