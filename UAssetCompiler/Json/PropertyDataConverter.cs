using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace UAssetCompiler.Json.Converter
{
    class PropertyDataConverter : JsonConverter
    {
        private readonly UAssetJsonGenerator? _generator;
        private readonly bool _isSerialize;

        public PropertyDataConverter()
        {
        }

        public PropertyDataConverter(UAssetJsonGenerator generator, bool isSerialize)
        {
            _generator = generator;
            _isSerialize = isSerialize;
        }

        public override bool CanRead => true;

        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType)
        {
            if (_isSerialize)
            {
                var result = objectType == typeof(PropertyData);
                //Console.WriteLine(@$"CanConvert: {objectType} result: {result}");
                return result;
            }
            else
            {
                var result = false;
                //Console.WriteLine(@$"CanConvert: {objectType}");
                switch (objectType.ToString())
                {
                    case "UAssetAPI.PropertyTypes.Objects.ObjectPropertyData":
                        result = true;
                        break;
                    case "UAssetAPI.PropertyTypes.Objects.NamePropertyData":
                        result = true;
                        break;
                    case "UAssetAPI.PropertyTypes.Objects.IntPropertyData":
                        result = true;
                        break;
                    case "UAssetAPI.PropertyTypes.Structs.GuidPropertyData":
                        result = true;
                        break;
                    case "UAssetAPI.PropertyTypes.Structs.FloatPropertyData":
                        result = true;
                        break;
                    case "UAssetAPI.PropertyTypes.Objects.BoolPropertyData":
                        result = true;
                        break;
                }

                return result;
            }
        }

        public override void WriteJson(JsonWriter writer, object? obj, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("$type");
            writer.WriteValue(obj!.GetType().Name);
            switch (obj)
            {
                case ObjectPropertyData objectPropertyData:
                    writer.WritePropertyName(objectPropertyData.Name.ToString()!);
                    writer.WriteValue(_generator!.IndexToToken(objectPropertyData.Value.Index));
                    break;

                case NamePropertyData namePropertyData:
                    writer.WritePropertyName(namePropertyData.Name.ToString()!);
                    writer.WriteValue(namePropertyData.Value);
                    break;

                case IntPropertyData intPropertyData:
                    writer.WritePropertyName(intPropertyData.Name.ToString()!);
                    writer.WriteValue(intPropertyData.Value);
                    break;

                case GuidPropertyData guidPropertyData:
                    writer.WritePropertyName(guidPropertyData.Name.ToString()!);
                    writer.WriteValue(guidPropertyData.Value);
                    break;

                case FloatPropertyData floatPropertyData:
                    writer.WritePropertyName(floatPropertyData.Name.ToString()!);
                    writer.WriteValue(floatPropertyData.Value);
                    break;

                case BoolPropertyData boolPropertyData:
                    writer.WritePropertyName(boolPropertyData.Name.ToString()!);
                    writer.WriteValue(boolPropertyData.Value);
                    break;
            }

            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue,
            JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var typeName = jsonObject.GetValue("$type")!.Value<string>();
            //Console.WriteLine(@$"ReadJson: {typeName}");

            if (typeName!.StartsWith("UAssetAPI.PropertyTypes"))
            {
                var list = typeName.Split(".");
                _generator!.Asset.AddNameReference(list[3].Replace("Data, UAssetAPI", "")) ;
            }
            else if (typeName.EndsWith("PropertyData"))
            {
                _generator!.Asset.AddNameReference(typeName.Replace("Data", "")) ;
            }


            var secondProperty = jsonObject.Properties().ElementAt(1);
            switch (typeName)
            {
                case "ObjectPropertyData":
                    var index = _generator!.TokenToIndex(secondProperty.Value.ToString());
                    return new ObjectPropertyData
                    {
                        Name = new FName(_generator!.Asset, secondProperty.Name),
                        Value = new FPackageIndex(index)
                    };

                case "NamePropertyData":
                    return new NamePropertyData
                    {
                        Name = new FName(_generator!.Asset, secondProperty.Name),
                        Value = new FName(_generator!.Asset, secondProperty.Value<string>())
                    };

                case "IntPropertyData":
                    return new IntPropertyData
                    {
                        Name = new FName(_generator!.Asset, secondProperty.Name),
                        Value = secondProperty.First!.Value<int>()
                    };

                case "BoolPropertyData":
                    return new BoolPropertyData
                    {
                        Name = new FName(_generator!.Asset, secondProperty.Name),
                        Value = secondProperty.First!.Value<bool>()
                    };

                case "GuidPropertyData":
                    return new GuidPropertyData
                    {
                        Name = new FName(_generator!.Asset, secondProperty.Name),
                        Value = new Guid(secondProperty.First!.Value<string>()!)
                    };

                case "FloatPropertyData":
                    return new FloatPropertyData
                    {
                        Name = new FName(_generator!.Asset, secondProperty.Name),
                        Value = secondProperty.First!.Value<float>()
                    };
            }

            return serializer.Deserialize(jsonObject.CreateReader())!;
        }
    }
}