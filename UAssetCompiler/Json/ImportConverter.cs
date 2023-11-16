using System.Text;
using System.Text.RegularExpressions;
using UAssetAPI;
using UAssetAPI.UnrealTypes;

namespace UAssetCompiler.Json;

public class ImportConverter
{
    private readonly UAsset _asset;

    public ImportConverter(UAsset asset)
    {
        _asset = asset;
    }

    private int? FindImport(string name)
    {
        var result = _asset.Imports
            .Select((import, index) => new { Import = import, Index = index })
            .FirstOrDefault(x => x.Import.ObjectName.ToString() == name);

        return -result?.Index - 1;
    }
    
    public string ImportToToken(Import import)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append($"{import.ObjectName}({import.ClassName})");

        var outerIndex = import.OuterIndex;
        var outerImport = _asset.GetImport(outerIndex.Index);
        builder.Append($"->{outerImport!.ObjectName}");

        return builder.ToString();
    }

    public void TokenToImports(string token)
    {
        Regex regex = new Regex(@"([\w_]+)\(([\w_]+)\)->([\w_/]+)");
        Match match = regex.Match(token);

        if (!match.Success) 
            throw new Exception("Import Token Error:" + token);
        
        var objName = match.Groups[1].Value;
        var className = match.Groups[2].Value;
        var packageName = match.Groups[3].Value;

        var result = FindImport(packageName);
        int outerIndex;
        if (result is null)
        {
            // make outer import
            var outerImport = new Import(
                new FName(_asset, "/Script/CoreUObject"),
                new FName(_asset, "Package"),
                new FPackageIndex(),
                new FName(_asset, packageName),
                false
            );
            outerIndex = _asset.AddImport(outerImport).Index;
        }
        else
        {
            outerIndex = result.Value;
        }

        packageName = className == "Class" ? "/Script/CoreUObject" : "/Script/FSD";
        _asset.Imports.Add(new Import(
            new FName(_asset, packageName),
            new FName(_asset, className),
            new FPackageIndex(outerIndex),
            new FName(_asset, objName),
            false
        ));
    }
}