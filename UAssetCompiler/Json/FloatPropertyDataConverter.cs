using Newtonsoft.Json;
using UAssetAPI.PropertyTypes.Objects;

namespace UAssetCompiler.Json;

public class FloatPropertyDataConverter : JsonConverter
{
    public override bool CanRead
    {
        get { return true; }
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(FloatPropertyData);
    }

    public override void WriteJson(JsonWriter writer, object obj, JsonSerializer serializer)
    {
        var floatPropertyData = (FloatPropertyData)obj;
        writer.WriteStartObject();
        writer.WritePropertyName(floatPropertyData.Name.ToString());
        writer.WriteValue(floatPropertyData.Value);
        writer.WriteEndObject();
    }


    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
        //return Convert.ToString(reader.Value).ConvertToGUID();
    }
}