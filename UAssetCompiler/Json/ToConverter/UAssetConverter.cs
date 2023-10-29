using Newtonsoft.Json;

namespace UAssetCompiler.Json.ToConverter;

public class UAssetConverter: JsonConverter
{
    public List<string> Imports = new();

    public List<UacExport> Exports = new();
    
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override bool CanConvert(Type objectType)
    {
        Console.WriteLine(objectType);
        
        return true;
    }
}
