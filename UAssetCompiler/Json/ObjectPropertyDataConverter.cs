using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UAssetAPI.PropertyTypes.Objects;

namespace UAssetCompiler.Json
{
    class ObjectPropertyDataConverter : JsonConverter
    {
        private UAssetJsonGenerator _generator;

        public ObjectPropertyDataConverter()
        {
        }

        public ObjectPropertyDataConverter(UAssetJsonGenerator generator)
        {
            _generator = generator;
        }

        public override bool CanRead => true;

        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType)
        {
            return true;
            //Console.WriteLine(objectType);
            //return objectType == typeof(ObjectPropertyData);
        }

        public override void WriteJson(JsonWriter writer, object? obj, JsonSerializer serializer)
        {
            var objectProperty = (ObjectPropertyData)obj!;
            writer.WriteStartObject();
            writer.WritePropertyName("$type");
            writer.WriteValue(objectProperty.GetType() + ", UAssetAPI");
            writer.WritePropertyName(objectProperty.Name.ToString());
            writer.WriteValue(_generator.IndexToToken(objectProperty.Value.Index));
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue,
            JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var typeName = jsonObject.GetValue("$type").Value<string>();
            Console.WriteLine("ReadJson" + typeName);

            //throw new NotImplementedException();
            //return Convert.ToString(reader.Value).ConvertToGUID();

            return null;
        }
    }
}