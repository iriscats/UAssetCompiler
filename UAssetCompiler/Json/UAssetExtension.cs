using System.Reflection;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.UnrealTypes;

namespace UAssetCompiler.Json;

public static class UAssetExtension
{
    public static Import? GetImport(this UAsset uAsset, int index) =>
        index > -1 ? null : uAsset.Imports[index * -1 - 1];

    public static Export? GetExport(this UAsset uAsset, int index) => 
        index < 1 ? null : uAsset.Exports[index - 1];


    public static int GetExportIndex(this UAsset uAsset, string objectName) =>
        uAsset.Exports.Select((export, index) => new { Export = export, Index = index })
            .First(x => x.Export.ObjectName.ToString() == objectName).Index + 1;

    public static string FindMainPackage(this UAsset uAsset)
    {
        return uAsset.GetNameMapIndexList()
            .First(x => x.ToString()!.EndsWith("/" + uAsset.Exports[0].ObjectName))
            .ToString()!;
    }

    public static void MakeDependsMap(this UAsset uAsset)
    {
        uAsset.DependsMap = new List<int[]>();
        for (int i = 0; i < uAsset.Exports.Count; i++)
        {
            uAsset.DependsMap.Add(Array.Empty<int>());
        }
    }

    public static void MakeGenerations(this UAsset uAsset)
    {
        uAsset.Generations = new List<FGenerationInfo>
        {
            new(uAsset.Exports.Count, uAsset.GetNameMapIndexList().Count)
        };
    }

    public static void MakeUAssetHeader(this UAsset uAsset)
    {
        uAsset.ClearNameIndexList();

        uAsset.LegacyFileVersion = -7;
        uAsset.UsesEventDrivenLoader = true;
        uAsset.WillSerializeNameHashes = true;
        uAsset.DependsMap = new List<int[]>();

        uAsset.PackageFlags = EPackageFlags.PKG_FilterEditorOnly;
        uAsset.FolderName = new FString("None");
        uAsset.AddNameReference("None");

        uAsset.SoftPackageReferenceList = new List<FString>();
        uAsset.ChunkIDs = Array.Empty<int>();
        uAsset.IsUnversioned = true;
        uAsset.UseSeparateBulkDataFiles = true;
        uAsset.SetDoWeHaveWorldTileInfo(false);
        uAsset.SetAdditionalPackagesToCook(new List<FString>());
        uAsset.SetDoWeHaveSoftPackageReferences(false);
        uAsset.AssetRegistryData = new byte[] { 0x00, 0x00, 0x00, 0x00 };
    }

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