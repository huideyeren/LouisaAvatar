using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XWear.IO.XResource.Util.UnityGenericJsonUtil
{
    public class KeyframeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var m = (UnityEngine.Keyframe)value;

            writer.WriteStartObject();
            writer.WritePropertyName("time");
            writer.WriteValue(m.time);

            writer.WritePropertyName("value");
            writer.WriteValue(m.value);

            writer.WritePropertyName("inTangent");
            writer.WriteValue(m.inTangent);
            writer.WritePropertyName("outTangent");
            writer.WriteValue(m.outTangent);

            writer.WritePropertyName("weightedMode");
            writer.WriteValue(m.weightedMode);

            writer.WritePropertyName("inWeight");
            writer.WriteValue(m.inWeight);
            writer.WritePropertyName("outWeight");
            writer.WriteValue(m.outWeight);

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
                return new UnityEngine.Keyframe();
            }

            var obj = JObject.Load(reader);
            return new UnityEngine.Keyframe()
            {
                time = (float)obj["time"],
                value = (float)obj["value"],
                inTangent = (float)obj["inTangent"],
                outTangent = (float)obj["outTangent"],
                weightedMode = (UnityEngine.WeightedMode)(int)obj["weightedMode"],
                inWeight = (float)obj["inWeight"],
                outWeight = (float)obj["outWeight"],
            };
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(UnityEngine.Keyframe);
        }
    }
}
