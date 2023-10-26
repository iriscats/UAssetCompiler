using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UAssetAPI.PropertyTypes.Objects;

namespace UAssetCompiler.Json
{
    internal class IntPropertyDataConverter : JsonConverter
    {

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IntPropertyData);
        }

        public override void WriteJson(JsonWriter writer, object obj, JsonSerializer serializer)
        {
            var intProperty = (IntPropertyData)obj;
            writer.WriteStartObject();
            writer.WritePropertyName(intProperty.Name.ToString());
            writer.WriteValue(intProperty.Value);
            writer.WriteEndObject();
        }


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
            //return Convert.ToString(reader.Value).ConvertToGUID();
        }

    }
}
