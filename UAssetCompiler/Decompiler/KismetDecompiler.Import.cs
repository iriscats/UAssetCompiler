using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.FieldTypes;
using UAssetAPI.IO;
using UAssetAPI.Kismet.Bytecode;
using UAssetAPI.Kismet.Bytecode.Expressions;
using UAssetAPI.UnrealTypes;
using UAssetCompiler.Utils;

namespace UAssetCompiler.Decompiler;

public partial class KismetDecompiler
{
    private List<Import> _imports;

    private void ProcessImport(Import import)
    {
        var rawImportIndex = _imports.IndexOf(import);
        var importIndex = rawImportIndex != -1 ? -(_imports.IndexOf(import) + 1) : -1;
        var children = _imports.Where(x => x.OuterIndex.Index == importIndex).ToList();
        var className = import.ClassName.ToString();
        var objectName = import.ObjectName.ToString();

        if (className == "ArrayProperty" && children.Any())
        {
            if (children.Count() != 1)
                throw new NotImplementedException();

            _writer.Write($"Array<{GetDecompiledTypeName(children.First())}> {FormatIdentifier(objectName)}");
        }
        else if (className == "Package")
        {
            var packageName = $"{FormatString(objectName)}";
            _writer.Write($"from {packageName} import");
        }
        else if (className == "Function")
        {
            if (objectName == "Default__Function")
            {
                _writer.Write($"Function Default__Function");
            }
            else
            {
                var functionTokens = new[]
                {
                    EExprToken.EX_FinalFunction, EExprToken.EX_LocalFinalFunction,
                    EExprToken.EX_LocalVirtualFunction, EExprToken.EX_VirtualFunction, EExprToken.EX_CallMath
                };
                var functionCalls = _asset.Exports
                    .Where(x => x is FunctionExport)
                    .Cast<FunctionExport>()
                    .SelectMany(x => x.ScriptBytecode.Flatten())
                    .Where(x => functionTokens.Contains(x.Token))
                    .Select(x => new
                    {
                        x.Token,
                        (x as EX_FinalFunction)?.StackNode,
                        Name = x switch
                        {
                            EX_FinalFunction funcExpr => _asset.GetName(funcExpr.StackNode),
                            EX_VirtualFunction funcExpr => funcExpr.VirtualFunctionName.ToString(),
                        }
                    });

                var callInstructions = functionCalls
                    .Where(x =>
                        (x.StackNode != null && x.StackNode.IsImport() &&
                         x.StackNode.ToImport(_asset) == import) ||
                        (x.StackNode == null && x.Name == import.ObjectName.ToString()))
                    .ToList();
                if (callInstructions.Any(x => x.StackNode != null))
                    callInstructions.RemoveAll(x => x.StackNode == null);

                var callInstructionTokens = callInstructions
                    .Select(x => x.Token)
                    .Distinct()
                    .ToList();

                var functionModifiers = new List<string>() { "public" };
                var functionAttributes = new List<string>() { "Extern", "UnknownSignature" };
                if (callInstructionTokens.Count > 0)
                {
                    var callInstruction = callInstructionTokens.First();
                    if (callInstructionTokens.Count > 1)
                    {
                        if (callInstructionTokens.All(x =>
                                x == EExprToken.EX_CallMath || x == EExprToken.EX_FinalFunction))
                        {
                            // TODO
                            callInstruction = EExprToken.EX_CallMath;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }

                    var functionModifier = callInstruction switch
                    {
                        EExprToken.EX_FinalFunction => "sealed",
                        EExprToken.EX_LocalFinalFunction => "sealed",
                        EExprToken.EX_LocalVirtualFunction => "virtual",
                        EExprToken.EX_VirtualFunction => "virtual",
                        EExprToken.EX_CallMath => "static sealed",
                    };
                    functionModifiers.Add(functionModifier);
                    var functionAttribute = callInstruction switch
                    {
                        EExprToken.EX_LocalFinalFunction => "CalledLocally",
                        EExprToken.EX_LocalVirtualFunction => "CalledLocally",
                        _ => ""
                    };
                    if (!string.IsNullOrWhiteSpace(functionAttribute))
                        functionAttributes.Add(functionAttribute);
                }

                var functionAttributeText = string.Join(", ", functionAttributes);
                if (!string.IsNullOrWhiteSpace(functionAttributeText))
                    functionAttributeText = $"[{functionAttributeText}] ";

                var functionModifierText = string.Join(" ", functionModifiers);
                if (!string.IsNullOrWhiteSpace(functionModifierText))
                    functionModifierText = $"{functionModifierText} ";

                _writer.Write(
                    $"{functionAttributeText}{functionModifierText}void {FormatIdentifier(import.ObjectName.ToString())}()");
            }
        }
        else if (_asset.ImportInheritsType(import, "Class"))
        {
            if (className == "Class")
            {
                // Try to detect the base class based on the members accessed
                // TODO: replace this with an evaluation phase where the code is evaluated
                // and context is used to deduce which base class each type has

                // Find properties of the imported type
                var targetProperties = _asset.Exports
                    .Where(x => x is PropertyExport)
                    .Cast<PropertyExport>()
                    .Where(x =>
                        (x.Property as UObjectProperty)?.PropertyClass.Index == importIndex)
                    .ToList();

                // Look for context expressions on these properties
                bool IsTargetProperty(KismetPropertyPointer ptr)
                {
                    if (ptr.Old != null)
                    {
                        return ptr.Old.IsExport() &&
                               targetProperties.Contains((ptr.Old.ToExport(_asset) as PropertyExport)!);
                    }
                    else
                    {
                        return ptr.New.ResolvedOwner.IsExport() &&
                               targetProperties.Contains(
                                   (ptr.New.ResolvedOwner.ToExport(_asset) as PropertyExport)!);
                    }
                }

                Import GetImportFromProperty(KismetPropertyPointer ptr)
                {
                    return ptr.Old != null ? ptr.Old.ToImport(_asset) : ptr.New.ResolvedOwner.ToImport(_asset);
                }


                var baseClassImport = _asset.Exports
                    .Where(x => x is FunctionExport)
                    .Cast<FunctionExport>()
                    .SelectMany(x => x.ScriptBytecode)
                    .Flatten()
                    .Where(x =>
                        // Find a context expression
                        x is EX_Context context &&

                        // ..where the target property is accessed through local variable
                        context.ObjectExpression is EX_LocalVariable local &&
                        IsTargetProperty(local.Variable) &&

                        // ..and the member being accessed is an instance variable
                        // given that the import does not have any children, this must be the base class
                        context.ContextExpression is EX_InstanceVariable)
                    .Select(x =>
                        GetImportFromProperty(((EX_InstanceVariable)((EX_Context)x).ContextExpression)
                            .Variable))
                    .Select(x => x.OuterIndex.ToImport(_asset))
                    .Distinct()
                    .SingleOrDefault();

                _writer.Write(
                    baseClassImport != null
                        ? $"class {FormatIdentifier(objectName)} : {FormatIdentifier(baseClassImport.ObjectName.ToString())}"
                        : $"class {FormatIdentifier(objectName)}");
            }
            else
            {
                _writer.Write(children.Any()
                    ? $"class {FormatIdentifier(objectName)} : {(GetDecompiledTypeName(import))}"
                    : $"{(GetDecompiledTypeName(import))} {FormatIdentifier(objectName)}");
            }
        }
        else if (_asset.ImportInheritsType(import, "Struct"))
        {
            if (className == "Struct")
            {
                _writer.Write($"struct {FormatIdentifier(objectName)}");
            }
            else
            {
                _writer.Write(children.Any()
                    ? $"struct {FormatIdentifier(objectName)} : {(GetDecompiledTypeName(import))}"
                    : $"{(GetDecompiledTypeName(import))} {FormatIdentifier(objectName)}");
            }
        }
        else
        {
            _writer.Write(
                $"{(GetDecompiledTypeName(import))} {FormatIdentifier(import.ObjectName.ToString())}");
        }

        if (children.Any())
        {
            _writer.WriteLine(" {");
            _writer.Push();
            foreach (var subImport in children)
            {
                ProcessImport(subImport);
            }

            _writer.Pop();
            _writer.WriteLine("}");
        }
        else
        {
            _writer.WriteLine(";");

            if (objectName.EndsWith("_GEN_VARIABLE"))
            {
                // TODO: verify that this is correct
                // Verified to be necessary for matching compilation
                // Similar patterns are seen in the Unreal source code as well
                var temp = new Import()
                {
                    bImportOptional = import.bImportOptional,
                    ClassName = import.ClassName,
                    ClassPackage = import.ClassPackage,
                    ObjectName = FName.DefineDummy(_asset,
                        import.ObjectName.ToString().Replace("_GEN_VARIABLE", "")),
                    OuterIndex = import.OuterIndex
                };
                ProcessImport(temp);
            }
        }
    }

    public void WriteImports()
    {
        _writer.WriteLine("// Imports");

        if (_asset is UAsset asset)
            _imports = asset.Imports;
        else if (_asset is ZenAsset zenAsset)
            _imports = zenAsset.Imports.Select(x => x.ToImport(zenAsset))
                .ToList();
        else
            throw new NotImplementedException("Unknown UAsset type");

        foreach (var import in _imports.Where(x => x.OuterIndex.Index == 0))
        {
            ProcessImport(import);
        }

        _writer.WriteLine();
    }
}