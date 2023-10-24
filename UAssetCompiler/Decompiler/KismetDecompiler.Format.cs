using System.Text.RegularExpressions;
using UAssetCompiler.Utils;

namespace UAssetCompiler.Decompiler;

public partial class KismetDecompiler
{
    private string FormatIdentifier(string name, bool allowKeywords = false)
    {
        if (!IdentifierRegex().IsMatch(name) ||
            (!allowKeywords && KismetScriptParser.IsKeyword(name)))
            return $"``{name}``";

        return name;
    }

    private string FormatString(string value)
    {
        if (value.Contains("\\"))
            value = value.Replace("\\", "\\\\");
        return $"\"{value}\"";
    }


    private string FormatCodeOffset(uint codeOffset, string? functionName = null) =>
        FormatIdentifier($"{(functionName ?? _function.ObjectName.ToString())}_{codeOffset}");

    private static Regex IdentifierRegex() => new("^[A-Za-z_][A-Za-z_\\d]*$");
}