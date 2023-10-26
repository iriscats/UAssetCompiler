using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAssetAPI.PropertyTypes.Objects;

namespace UAssetCompiler.Json
{
    class ObjectPropertyDataConverter : JsonConverter
    {
        private UAssetJsonGenerator _generator;

        public ObjectPropertyDataConverter(UAssetJsonGenerator generator)
        {
            _generator = generator;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ObjectPropertyData);
        }

        public override void WriteJson(JsonWriter writer, object obj, JsonSerializer serializer)
        {
            var objectProperty = (ObjectPropertyData)obj;
            writer.WriteStartObject();
            writer.WritePropertyName(objectProperty.Name.ToString());
            writer.WriteValue(_generator.IndexToToken(objectProperty.Value.Index));
            writer.WriteEndObject();
        }


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
            //return Convert.ToString(reader.Value).ConvertToGUID();
        }

    }
}
