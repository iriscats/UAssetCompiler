using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace UAssetCompiler.Decompiler;

public class UAssetScriptGenerator
{
    private readonly UAsset _asset;
    private readonly IndentedWriter _writer = new();

    public UAssetScriptGenerator(string path)
    {
        _asset = new UAsset(path, EngineVersion.VER_UE4_27);
    }

    public UAssetScriptGenerator(UAsset asset)
    {
        _asset = asset;
    }

    Import? GetImport(int index) => index > -1 ? null : _asset.Imports[index * -1 - 1];

    Export? GetExport(int index) => index < 1 ? null : _asset.Exports[index - 1];

    string GetIndexName(int index)
    {
        if (index == 0)
        {
            return "NullObjectRef";
        }

        var import = GetImport(index);
        if (import == null)
        {
            var export = GetExport(index);
            if (export == null)
            {
                return "Error Index:" + index;
            }

            return export.ObjectName.ToString();
        }

        return import.ObjectName.ToString();
    }

    void PrintImports()
    {
        foreach (var import in _asset.Imports)
        {
            _writer.Write($@"import ""{import.ObjectName}""(""{import.ClassPackage}/{import.ClassName}"")");

            var outerIndex = import.OuterIndex;
            if (outerIndex.Index != 0)
            {
                var outerImport = GetImport(outerIndex.Index);
                _writer.Write($@" from ""{outerImport!.ObjectName}""");
            }
            else
            {
                _writer.Write(@" from ""Package""");
            }

            _writer.WriteLine(@";");
        }
    }


    void PrintValue(PropertyData data)
    {
        switch (data.PropertyType.ToString())
        {
            case "ArrayProperty":
                PrintArrayProperty((data as ArrayPropertyData)!);
                break;
            case "NameProperty":
                PrintNameProperty((data as NamePropertyData)!);
                break;
            case "StructProperty":
                PrintStructProperty((data as StructPropertyData)!);
                break;
            case "ObjectProperty":
                PrintObjectProperty((data as ObjectPropertyData)!);
                break;
            case "FloatProperty":
                PrintFloatProperty((data as FloatPropertyData)!);
                break;
            case "IntProperty":
                PrintIntProperty((data as IntPropertyData)!);
                break;
            case "BoolProperty":
                PrintBoolProperty((data as BoolPropertyData)!);
                break;
            case "ByteProperty":
                PrintByteProperty((data as BytePropertyData)!);
                break;
            case "UInt16Property":
                PrintUInt16Property((data as UInt16PropertyData)!);
                break;
            case "EnumProperty":
                PrintEnumProperty((data as EnumPropertyData)!);
                break;
            case "MapProperty":
                PrintMapProperty((data as MapPropertyData)!);
                break;
            case "UnknownProperty":
                PrintUnknownProperty((data as UnknownPropertyData)!);
                break;
            default:
                _writer.Write(@"Unknown Property Type!" + data.PropertyType);
                break;
        }
    }

    void PrintFloatProperty(FloatPropertyData data)
    {
        _writer.Write(@$"{data.Value}f");
    }

    void PrintObjectProperty(ObjectPropertyData data)
    {
        var name = GetIndexName(data.Value.Index);
        _writer.Write($@"{name}");
    }

    void PrintIntProperty(IntPropertyData data)
    {
        _writer.Write(@$"{data.Value}");
    }

    void PrintNameProperty(NamePropertyData data)
    {
        _writer.Write($@"""{data.Value}""");
    }

    void PrintBoolProperty(BoolPropertyData data)
    {
        _writer.Write($@"{data.Value}");
    }

    void PrintByteProperty(BytePropertyData data)
    {
        _writer.Write($@"{data.Value}b");
    }

    void PrintUInt16Property(UInt16PropertyData data)
    {
        _writer.Write($@"{data.Value}u16");
    }

    void PrintEnumProperty(EnumPropertyData data)
    {
        _writer.Write($@"{data.Value}");
    }

    void PrintUnknownProperty(UnknownPropertyData data)
    {
        string encoded = Convert.ToBase64String(data.Value);
        _writer.Write($"UnknownProperty(\"{encoded}\")");
    }

    void PrintMapProperty(MapPropertyData data)
    {
        _writer.WriteLine("MapProperty(");
        _writer.Push();
        _writer.Write("no supported!" + data);
        _writer.Pop();
        _writer.Write(")");
    }


    void PrintArrayProperty(ArrayPropertyData data)
    {
        _writer.WriteLine("[");
        _writer.Push();

        for (int i = 0; i < data.Value.Length; i++)
        {
            var item = data.Value[i];
            PrintValue(item);

            if (i != data.Value.Length - 1)
            {
                _writer.Write(@",");
            }

            _writer.WriteLine();
        }

        _writer.Pop();
        _writer.Write("]");
    }

    void PrintStructProperty(StructPropertyData data)
    {
        _writer.WriteLine("(");
        _writer.Push();

        for (int i = 0; i < data.Value.Count; i++)
        {
            var item = data.Value[i];

            _writer.Write(@$"{item.Name} = ");
            PrintValue(item);

            if (i != data.Value.Count - 1)
            {
                _writer.Write(@",");
            }

            _writer.WriteLine();
        }

        _writer.Pop();
        _writer.Write(")");
    }

    void PrintProperty(PropertyData data)
    {
        _writer.Write($@"{data.Name} = ");
        PrintValue(data);
        _writer.WriteLine(";");
    }

    void PrintExport()
    {
        foreach (var export in _asset.Exports)
        {
            var objectName = GetIndexName(export.ClassIndex.Index);
            _writer.Write($@"export {export.ObjectName}: {objectName} = ");
            _writer.WriteLine("{");
            _writer.Push();

            if (export is NormalExport normalExport)
            {
                foreach (var data in normalExport.Data)
                {
                    PrintProperty(data);
                }
            }
            else if (export is ClassExport classExport)
            {
                foreach (var data in classExport.Data)
                {
                    PrintProperty(data);
                }
            }
            else
            {
                _writer.WriteLine(@"Unknown Export Type!");
            }

            _writer.Pop();
            _writer.WriteLine("}");
            _writer.WriteLine();
        }
    }

    public string? MakeScript()
    {
        var kismet = new KismetDecompiler(_writer, _asset);
        kismet.WriteImports();
        kismet.WriteExports();
        
        PrintImports();
        _writer.WriteLine();

        PrintExport();
        _writer.WriteLine();

        return _writer.ToString();
    }
}