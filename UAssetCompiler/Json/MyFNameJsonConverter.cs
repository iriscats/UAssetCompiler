using Newtonsoft.Json;
using UAssetAPI;
using UAssetAPI.UnrealTypes;

namespace UAssetCompiler.Json;

public class MyFNameJsonConverter : JsonConverter
{
    private UAsset _asset;

    public MyFNameJsonConverter(UAsset asset)
    {
        _asset = asset;
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(FName);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
    }

    public override bool CanRead => true;

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.Value == null)
            return null;

        return new FName(_asset, reader.Value.ToString());
    }
}