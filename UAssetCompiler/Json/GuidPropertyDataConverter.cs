using Newtonsoft.Json;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;

namespace UAssetCompiler.Json;

public class GuidPropertyDataConverter : JsonConverter
{
    public override bool CanRead
    {
        get { return true; }
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(GuidPropertyData);
    }

    public override void WriteJson(JsonWriter writer, object obj, JsonSerializer serializer)
    {
        var guidPropertyData = (GuidPropertyData)obj;
        writer.WriteStartObject();
        writer.WritePropertyName(guidPropertyData.Name.ToString());
        writer.WriteValue("[Guid]" + guidPropertyData.Value);
        writer.WriteEndObject();
    }


    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
        //return Convert.ToString(reader.Value).ConvertToGUID();
    }
}