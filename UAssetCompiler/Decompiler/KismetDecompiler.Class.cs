using UAssetAPI.ExportTypes;
using UAssetCompiler.Decompiler.Context.Properties;
using UAssetCompiler.Utils;

namespace UAssetCompiler.Decompiler;

public partial class KismetDecompiler
{
    private ClassExport _class;

    private void WriteFunctionClass(ClassExport clazz)
    {
        _class = clazz;
        var classBaseClass = _asset.GetName(_class.SuperStruct);
        var classChildExports = _class.Children
            .Select(x => x.ToExport(_asset))
            .ToList();

        var classProperties = classChildExports
            .Where(x => x is PropertyExport)
            .Cast<PropertyExport>()
            .Select(x => (IPropertyData)new PropertyExportData(x))
            .OrderBy(x => _asset.Exports.IndexOf((Export)x.Source))
            .Union(_class.LoadedProperties.Select(x => new FPropertyData(_asset, x)))
            .ToList();

        var classFunctions = classChildExports
            .Where(x => x is FunctionExport)
            .Cast<FunctionExport>()
            .OrderBy(x => _class.FuncMap.IndexOf(x.ObjectName));

        var classModifiers = GetClassModifiers(_class);
        var classAttributes = GetClassAttributes(_class);

        var modifierText = string.Join(" ", classModifiers).Trim();
        var attributeText = string.Join(", ", classAttributes).Trim();
        var nameText = $"class {_class.ObjectName} : {classBaseClass}";

        if (!string.IsNullOrWhiteSpace(attributeText))
            attributeText = $"[{attributeText}]";

        _writer.WriteLine(
            $"{string.Join(" ", new[] { attributeText, modifierText }.Where(x => !string.IsNullOrWhiteSpace(x)))}");
        _writer.WriteLine($"{nameText} {{");
        _writer.Push();

        foreach (var prop in classProperties)
        {
            _writer.WriteLine($"{GetDecompiledPropertyText(prop)};");
        }

        foreach (var fun in classFunctions)
        {
            DecompileFunction(fun);
        }

        _writer.Pop();
        _writer.WriteLine($"}}");
    }


    private void WriteDataClass(NormalExport clazz)
    {
        var classBaseClass = _asset.GetName(clazz.SuperIndex);

        var nameText = $"class {clazz.ObjectName} : {classBaseClass}";

        foreach (var propertyData in clazz.Data)
        {
            
            //_writer.WriteLine($"{GetDecompiledPropertyText(prop!)};");
        }
      
    }

}