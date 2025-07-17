using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.UnrealTypes;

namespace UAssetCompiler.Json;

public class ExportConverter
{
    private readonly UAsset _asset;
    private readonly UAssetJsonGenerator _generator;

    public ExportConverter(UAsset asset, UAssetJsonGenerator generator)
    {
        _asset = asset;
        _generator = generator;
    }

    public void SolvingDuplicateObjectName()
    {
        var exportDic = new Dictionary<string, Export>();
        for (var i = 0; i < _asset.Exports.Count; i++)
        {
            var export = _asset.Exports[i];
            var name = export.ObjectName.ToString()!;
            if (exportDic.ContainsKey(name))
            {
                export.ObjectName = new FName(_asset, $"{name}_{i}");
                _asset.Exports[i] = export;
            }
        }
    }

    private List<FPackageIndex> CollectAllDepends(UacExport export)
    {
        var list = new List<FPackageIndex>();
        if (export is UacNormalExport normalExport)
        {
            foreach (var propertyData in normalExport.Data)
            {
                switch (propertyData)
                {
                    case ObjectPropertyData objectPropertyData:
                        list.Add(objectPropertyData.Value);
                        break;
                    case MapPropertyData mapPropertyData:
                        foreach (var mapItem
                                 in mapPropertyData.Value)
                        {
                            if (mapItem.Key is ObjectPropertyData mapKeyObjectPropertyData)
                            {
                                list.Add(mapKeyObjectPropertyData.Value);
                            }

                            if (mapItem.Value is ObjectPropertyData mapValueObjectPropertyData)
                            {
                                list.Add(mapValueObjectPropertyData.Value);
                            }
                        }

                        break;
                }
            }
        }

        return list;
    }

    public UacExport ToUacExport(Export export)
    {
        UacExport uacExport;
        switch (export)
        {
            case FunctionExport functionExport:
                uacExport = new UacFunctionExport(functionExport);
                break;
            case ClassExport classExport:
                uacExport = new UacClassExport(classExport);
                break;
            case NormalExport normalExport:
                uacExport = new UacNormalExport(normalExport);
                break;
            case RawExport rawExport:
                uacExport = new UacRawExport(rawExport);
                break;
            default:
                throw new NotImplementedException("Export To Json");
        }

        uacExport.ObjectName = export.ObjectName.ToString()!;
        uacExport.OuterObject = _generator.IndexToToken(export.OuterIndex.Index);
        uacExport.SuperObject = _generator.IndexToToken(export.SuperIndex.Index);
        uacExport.Class = _generator.IndexToToken(export.ClassIndex.Index);
        uacExport.TemplateObject = _generator.IndexToToken(export.TemplateIndex.Index);
        uacExport.ObjectFlags = export.ObjectFlags.ToString();
        return uacExport;
    }

    public Export ToExport(UacExport uacExport)
    {
        var origin = uacExport as UacNormalExport;

        var dest = new NormalExport
        {
            ObjectName = new FName(_asset, origin!.ObjectName),
            Data = origin.Data,
            bNotAlwaysLoadedForEditorGame = true,
            OuterIndex = FPackageIndex.FromRawIndex(_generator.TokenToIndex(origin.OuterObject)),
            SuperIndex = FPackageIndex.FromRawIndex(_generator.TokenToIndex(origin.SuperObject)),
            TemplateIndex = FPackageIndex.FromRawIndex(_generator.TokenToIndex(origin.TemplateObject)),
            ClassIndex = FPackageIndex.FromRawIndex(_generator.TokenToIndex(origin.Class)),
            Extras = new byte[] { 0x00, 0x00, 0x00, 0x00 },
        };

        if (Enum.TryParse(origin.ObjectFlags, out EObjectFlags flags))
        {
            dest.ObjectFlags = flags;
        }

        dest.CreateBeforeSerializationDependencies.AddRange(CollectAllDepends(origin));
        dest.SerializationBeforeCreateDependencies =
            new List<FPackageIndex> { dest.ClassIndex, dest.TemplateIndex };
        if (dest.OuterIndex.Index != 0)
        {
            dest.CreateBeforeCreateDependencies.Add(dest.OuterIndex);
        }

        return dest;
    }
}