using Newtonsoft.Json;
using UAssetAPI.PropertyTypes.Objects;

namespace UAssetCompiler.Json;

public class NamePropertyDataConverter : JsonConverter
{
    public override bool CanRead
    {
        get { return true; }
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(NamePropertyData);
    }

    public override void WriteJson(JsonWriter writer, object obj, JsonSerializer serializer)
    {
        var namePropertyData = (NamePropertyData)obj;
        writer.WriteStartObject();
        writer.WritePropertyName("$type");
        writer.WriteValue(namePropertyData.GetType() + ", UAssetAPI");
        writer.WritePropertyName(namePropertyData.Name.ToString());
        writer.WriteValue(namePropertyData.Value);
        writer.WriteEndObject();
    }


    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
        //return Convert.ToString(reader.Value).ConvertToGUID();
    }
}