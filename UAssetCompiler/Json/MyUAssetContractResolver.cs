using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UAssetAPI;
using UAssetAPI.JSON;
using UAssetAPI.UnrealTypes;

namespace UAssetCompiler.Json;

public class MyUAssetContractResolver : DefaultContractResolver
{
    private readonly UAsset _asset;

    protected override JsonConverter ResolveContractConverter(Type objectType)
    {
        if (typeof(FName).IsAssignableFrom(objectType))
        {
            return new MyFNameJsonConverter(_asset);
        }

        return base.ResolveContractConverter(objectType);
    }

    public MyUAssetContractResolver(UAsset asset) : base()
    {
        _asset = asset;
    }
}