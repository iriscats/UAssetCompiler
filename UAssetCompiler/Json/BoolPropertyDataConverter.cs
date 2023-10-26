using Newtonsoft.Json;
using UAssetAPI.PropertyTypes.Objects;

namespace UAssetCompiler.Json;

public class BoolPropertyDataConverter : JsonConverter
{
    public override bool CanRead
    {
        get { return true; }
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(BoolPropertyData);
    }

    public override void WriteJson(JsonWriter writer, object obj, JsonSerializer serializer)
    {
        var boolPropertyData = (BoolPropertyData)obj;
        writer.WriteStartObject();
        writer.WritePropertyName(boolPropertyData.Name.ToString());
        writer.WriteValue(boolPropertyData.Value);
        writer.WriteEndObject();
    }


    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
        //return Convert.ToString(reader.Value).ConvertToGUID();
    }
}