using System.Diagnostics;
using UAssetAPI.ExportTypes;
using UAssetAPI.Kismet.Bytecode.Expressions;
using UAssetAPI.UnrealTypes;
using UAssetCompiler.Decompiler.Context;
using UAssetCompiler.Decompiler.Context.Nodes;
using UAssetCompiler.Decompiler.Context.Properties;
using UAssetCompiler.Decompiler.Passes;
using UAssetCompiler.Utils;

namespace UAssetCompiler.Decompiler;

public partial class KismetDecompiler
{
    private FunctionExport _function;
    private bool _isUbergraphFunction;

    private Node ExecutePass<T>(Node? root) where T : IDecompilerPass, new()
    {
        var pass = new T();
        return pass.Execute(new DecompilerContext()
        {
            Asset = _asset,
            Class = _class,
            Function = _function
        }, root!);
    }

    private void DecompileFunction(FunctionExport function)
    {
        _function = function;

        var root = ExecutePass<CreateBasicNodesPass>(null);
        root = ExecutePass<ResolveJumpTargetsPass>(root);
        root = ExecutePass<ResolveReferencesPass>(root);
        root = ExecutePass<CreateBasicBlocksPass>(root);
        root = ExecutePass<RemoveGotoReturnsPass>(root);
        root = ExecutePass<CreateIfBlocksPass>(root);

        if (!_verbose)
            WriteFunction(function, root);
        else
            WriteFunctionVerbose(function, root);

        _writer.Flush();
    }

    void WriteBlock(Node root)
    {
        var nextBlockIndex = 1;

        foreach (Node block in root.Children)
        {
            if (block is IfBlockNode ifBlock)
            {
                var isBlockStart = block.ReferencedBy.Count > 0 ||
                                   (_isUbergraphFunction && IsUbergraphEntrypoint(block.CodeStartOffset));

                if (isBlockStart)
                    _writer.WriteLine($"{FormatCodeOffset((uint)block.CodeStartOffset)}:");

                var cond = FormatExpression(ifBlock.Condition, null);
                _writer.WriteLine($"if ({cond}) {{");
                _writer.Push();
                WriteBlock(ifBlock);
                _writer.Pop();
                _writer.WriteLine($"}}");
                _writer.WriteLine();
            }
            else
            {
                _writer.WriteLine($"// Block {nextBlockIndex++}");
                foreach (var node in block.Children)
                {
                    if (node is ReturnNode returnNode)
                    {
                        if (returnNode.Source is EX_Jump)
                        {
                            WriteExpression(node, _isUbergraphFunction, "return");
                        }
                        else if (returnNode.Source is EX_JumpIfNot jumpIfNot)
                        {
                            WriteExpression(node, _isUbergraphFunction,
                                $"if (!{FormatExpression(jumpIfNot.BooleanExpression, null)}) return");
                        }
                        else
                        {
                            WriteExpression(node, _isUbergraphFunction);
                        }
                    }
                    else
                    {
                        WriteExpression(node, _isUbergraphFunction);
                    }
                }
            }

            _writer.WriteLine();
        }
    }


    private void WriteFunction(FunctionExport function, Node root)
    {
        var functionFlags =
            typeof(EFunctionFlags)
                .GetEnumValues()
                .Cast<EFunctionFlags>()
                .Where(x => (function.FunctionFlags & x) != 0 && x != EFunctionFlags.FUNC_AllFlags);

        var functionModifierFlags = new[]
        {
            EFunctionFlags.FUNC_Final, EFunctionFlags.FUNC_Static, EFunctionFlags.FUNC_Public,
            EFunctionFlags.FUNC_Private, EFunctionFlags.FUNC_Protected
        };

        var functionModifiers = functionFlags.Where(x => functionModifierFlags.Contains(x))
            .Select(x => x.ToString().Replace("FUNC_", "").ToLower())
            .Select(x => x == "final" ? "sealed" : x)
            .ToList();
        if (!function.SuperIndex.IsNull())
            functionModifiers.Add("override");

        var functionAttributes = functionFlags
            .Except(functionModifierFlags)
            .Select(x => x.ToString().Replace("FUNC_", ""))
            .ToList();

        //var functionProperties = function.Children != null ?
        //    function.Children.Select(x => x.ToExport(_asset)).Cast<PropertyExport>() :
        //    _asset.Exports.Where(x => !x.OuterIndex.IsNull() && x.OuterIndex.ToExport(_asset) == function)
        //    .Cast<PropertyExport>();

        var functionChildExports =
            _asset.Exports.Where(x => !x.OuterIndex.IsNull() && x.OuterIndex.ToExport(_asset) == function);

        var functionProperties =
            functionChildExports
                .Where(x => x is PropertyExport)
                .Select(x => (IPropertyData)new PropertyExportData((PropertyExport)x))
                .Union(function.LoadedProperties.Select(x => new FPropertyData(_asset, x)))
                .ToList();

        var functionParams = functionProperties
            .Where(x => (x.PropertyFlags & EPropertyFlags.CPF_Parm) != 0);

        var functionLocals = functionProperties
            .Except(functionParams);


        var functionParameterText =
            string.Join(", ", functionParams.Select(GetDecompiledPropertyText));

        _isUbergraphFunction = function.IsUbergraphFunction();

        var functionAttributeText = string.Join(", ", functionAttributes);

        if (!string.IsNullOrWhiteSpace(functionAttributeText))
        {
            _writer.WriteLine($"[{functionAttributeText}]");
        }

        var functionDeclaration =
            $"void {FormatIdentifier(function.ObjectName.ToString())}({functionParameterText}) {{";
        var functionModifierText = string.Join(" ", functionModifiers);
        if (!string.IsNullOrWhiteSpace(functionModifierText))
        {
            functionDeclaration = functionModifierText + " " + functionDeclaration;
        }

        _writer.WriteLine(functionDeclaration);
        _writer.Push();

        if (functionLocals.Any())
        {
            _writer.WriteLine($"// Locals");
            foreach (var functionLocal in functionLocals)
                _writer.WriteLine($"{GetDecompiledPropertyText(functionLocal)};");
            _writer.WriteLine();
        }

        WriteBlock(root);

        _writer.Pop();
        _writer.WriteLine("}\n");
    }

    private void WriteFunctionVerbose(FunctionExport function, Node root)
    {
        var isUbergraphFunction = function.IsUbergraphFunction();
        _writer.WriteLine($"void {function.ObjectName}() {{");

        var nextBlockIndex = 1;
        foreach (Node block in root.Children)
        {
            Debug.Assert(block.Source == null);
            _writer.WriteLine($"    // Block {nextBlockIndex++}");

            foreach (var node in block.Children)
            {
                string line;
                if (node.ReferencedBy.Count > 0 ||
                    (isUbergraphFunction && IsUbergraphEntrypoint(node.CodeStartOffset)))
                {
                    line = $"    _{node.CodeStartOffset}: {FormatExpression(node.Source, null)}";
                }
                else
                {
                    line = $"    {FormatExpression(node.Source, null)}";
                }

                if (line.Contains(" //"))
                {
                    var parts = line.Split(" //");
                    if (!string.IsNullOrWhiteSpace(parts[0]))
                        line = string.Join("; //", parts);
                }
                else if (line.Contains(" /*"))
                {
                    var parts = line.Split(" /*");
                    if (!string.IsNullOrWhiteSpace(parts[0]))
                        line = string.Join("; /*", parts);
                }
                else
                {
                    line += ";";
                }

                _writer.WriteLine(line);
            }

            _writer.WriteLine();
        }

        _writer.WriteLine("}\n");
    }
}