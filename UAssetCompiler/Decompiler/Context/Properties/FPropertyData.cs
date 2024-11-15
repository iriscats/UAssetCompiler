﻿using UAssetAPI;
using UAssetAPI.FieldTypes;
using UAssetAPI.UnrealTypes;
using UAssetCompiler.Utils;

namespace UAssetCompiler.Decompiler.Context.Properties;

public class FPropertyData : IPropertyData
{
    public UnrealPackage Asset { get; }
    public FProperty Source { get; }

    public FPropertyData(UnrealPackage asset, FProperty source)
    {
        this.Asset = asset;
        Source = source;
    }

    public string Name => Source.Name.ToString();
    public EPropertyFlags PropertyFlags => Source.PropertyFlags;
    public string TypeName => Source.SerializedType.ToString();

    public string? PropertyClassName
        => Asset.GetName(((FObjectProperty)Source).PropertyClass);

    public string? InterfaceClassName
        => Asset.GetName(((FInterfaceProperty)Source).InterfaceClass);

    public string? StructName
        => Asset.GetName(((FStructProperty)Source).Struct);

    public IPropertyData? ArrayInnerProperty
    {
        get
        {
            var inner = ((FArrayProperty)Source).Inner;
            if (inner != null)
            {
                return new FPropertyData(Asset, inner);
            }
            return null;
        }
    }

    object IPropertyData.Source => Source;
}