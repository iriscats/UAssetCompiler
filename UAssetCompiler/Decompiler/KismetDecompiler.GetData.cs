using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.Kismet.Bytecode;
using UAssetAPI.Kismet.Bytecode.Expressions;
using UAssetAPI.UnrealTypes;
using UAssetCompiler.Decompiler.Context.Properties;
using UAssetCompiler.Utils;

namespace UAssetCompiler.Decompiler;

public partial class KismetDecompiler
{
    private bool _useFullPropertyNames = false;
    private bool _useFullFunctionNames = false;
    private static EClassFlags[] classModifierFlags = { EClassFlags.CLASS_Abstract };
    private bool _verbose = false;

    private IEnumerable<EClassFlags> GetClassFlags(ClassExport classExport)
    {
        var classFlags = typeof(EClassFlags)
            .GetEnumValues()
            .Cast<EClassFlags>()
            .Where(x => (classExport.ClassFlags & x) != 0);
        return classFlags;
    }

    private List<string> GetClassModifiers(ClassExport classExport)
    {
        var classModifiers =
            GetClassFlags(classExport)
                .Where(x => classModifierFlags.Contains(x))
                .Select(GetModifierForClassFlag)
                .ToList();

        return classModifiers;
    }

    private List<string> GetClassAttributes(ClassExport classExport)
    {
        var classAttributes =
            GetClassFlags(classExport)
                .Except(classModifierFlags)
                .Select(x => x.ToString().Replace("CLASS_", "").Trim())
                .ToList();
        return classAttributes;
    }

    private string GetDecompiledTypeName(Import import)
    {
        var classType = import.ClassName.ToString();
        switch (classType)
        {
            case "Package":
                return "package";
            case "FloatProperty":
                return "float";
            case "IntProperty":
                return "int";
            case "StrProperty":
                return "string";
            case "BoolProperty":
                return "bool";
            case "ByteProperty":
                return "byte";
            case "Class":
                return "class";
            default:
                if (classType != "Property" &&
                    classType.EndsWith("Property"))
                    return FormatIdentifier(classType.Substring(0, classType.IndexOf("Property")));

                return FormatIdentifier(classType);
        }
    }

    private string GetDecompiledType(IPropertyData prop)
    {
        var classType = prop.TypeName;
        switch (classType)
        {
            case "IntProperty":
                return "int";
            case "StrProperty":
                return "string";
            case "FloatProperty":
                return "float";
            case "InterfaceProperty":
            {
                var interfaceName = prop.InterfaceClassName;
                return $"Interface<{interfaceName}>";
            }
            case "StructProperty":
            {
                var structName = prop.StructName;
                return $"Struct<{structName}>";
            }
            case "BoolProperty":
                return "bool";
            case "ByteProperty":
                return "byte";
            case "ArrayProperty":
            {
                if (prop.ArrayInnerProperty != null)
                {
                    return $"Array<{GetDecompiledType(prop.ArrayInnerProperty)}>";
                }

                // TODO
                return $"Array";
            }
            case "ObjectProperty":
            {
                return $"Object<{prop.PropertyClassName}>";
            }
            default:
                if (classType != "Property" &&
                    classType.EndsWith("Property"))
                    return classType.Substring(0, classType.IndexOf("Property"));

                return classType;
        }
    }

    private string GetModifierForPropertyFlag(EPropertyFlags flag)
    {
        switch (flag)
        {
            case EPropertyFlags.CPF_ConstParm:
                return "const";
            case EPropertyFlags.CPF_Parm:
                return "";
            case EPropertyFlags.CPF_OutParm:
                return "out";
            case EPropertyFlags.CPF_ReferenceParm:
                return "ref";
            default:
                throw new ArgumentOutOfRangeException(nameof(flag));
        }
    }

    private string GetModifierForClassFlag(EClassFlags flag)
    {
        switch (flag)
        {
            case EClassFlags.CLASS_Abstract:
                return "abstract";
            default:
                throw new ArgumentOutOfRangeException(nameof(flag));
        }
    }

    private string GetDecompiledPropertyText(IPropertyData prop)
    {
        var flags =
            typeof(EPropertyFlags)
                .GetEnumValues()
                .Cast<EPropertyFlags>()
                .Where(x => (prop.PropertyFlags & x) != 0 && x != EPropertyFlags.CPF_Parm);

        var modifierFlags = new[]
        {
            EPropertyFlags.CPF_ConstParm, EPropertyFlags.CPF_OutParm, EPropertyFlags.CPF_ReferenceParm
        };

        var modifiers = flags.Where(x => modifierFlags.Contains(x))
            .Select(GetModifierForPropertyFlag)
            .ToList();

        var attributes = flags
            .Except(modifierFlags)
            .Select(x => x.ToString().Replace("CPF_", "")
                .Replace("Param", "")
                .Replace("Parm", "").Trim())
            .ToList();


        //if (!prop.Property.PropertyFlags.HasFlag(EPropertyFlags.CPF_Parm))
        //{
        //    if (modifiers.Contains("ref"))
        //    {
        //        modifiers.Remove("ref");
        //        attributes.Add("Ref");
        //    }
        //}

        var modifierText = string.Join(" ", modifiers).Trim();
        var attributeText = string.Join(", ", attributes).Trim();
        var nameText = $"{GetDecompiledType(prop)} {FormatIdentifier(prop.Name)}";

        if (!string.IsNullOrWhiteSpace(attributeText))
            attributeText = $"[{attributeText}]";

        var result = string.Join(" ",
            new[] { attributeText, modifierText, nameText }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();
        return result;
    }

    private bool IsUbergraphEntrypoint(int codeOffset)
    {
        var entryPoints = _asset.Exports
            .Where(x => x is FunctionExport)
            .Cast<FunctionExport>()
            .SelectMany(x => x.ScriptBytecode)
            .Where(x => x.Token == EExprToken.EX_LocalFinalFunction)
            .Cast<EX_LocalFinalFunction>()
            .Where(x => x.StackNode.IsExport() && _asset.GetFunctionExport(x.StackNode).IsUbergraphFunction())
            .Select(x => x.Parameters[0] as EX_IntConst)
            .Select(x => x.Value);
        return entryPoints.Contains(codeOffset);
    }
    

    private string GetFunctionName(FPackageIndex index)
    {
        return _useFullFunctionNames ? _asset.GetFullName(index) : _asset.GetName(index);
    }
}