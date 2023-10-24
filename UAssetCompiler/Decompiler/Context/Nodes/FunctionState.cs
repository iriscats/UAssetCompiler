namespace UAssetCompiler.Decompiler.Context.Nodes;

public class FunctionState
{
    public HashSet<string> DeclaredVariables { get; init; } = new();
}
