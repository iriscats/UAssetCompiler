using System.Reflection;
using UAssetAPI;
using UAssetAPI.UnrealTypes;

namespace UAssetCompiler.Json;

public static class UAssetExtension
{
    public static int AddNameReference(this UAsset uAsset, string value)
    {
        return uAsset.AddNameReference(new FString(value));
    }

    public static void SetDoWeHaveWorldTileInfo(this UAsset uAsset, bool value)
    {
        FieldInfo? field = typeof(UAsset).GetField("doWeHaveWorldTileInfo",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field!.SetValue(uAsset, value);
    }

    public static void SetDoWeHaveSoftPackageReferences(this UAsset uAsset, bool value)
    {
        FieldInfo? field = typeof(UAsset).GetField("doWeHaveSoftPackageReferences",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field!.SetValue(uAsset, value);
    }

    public static void SetAdditionalPackagesToCook(this UAsset uAsset, List<FString> list)
    {
        FieldInfo? field = typeof(UAsset).GetField("AdditionalPackagesToCook",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field!.SetValue(uAsset, list);
    }
    
    public static int CountExportData(this UAsset uAsset)
    {
        int count = uAsset.GetNameMapIndexList().Count;
        FieldInfo? field = typeof(UAsset).GetField("NamesReferencedFromExportDataCount",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field!.SetValue(uAsset, count);
        return count;
    }
}