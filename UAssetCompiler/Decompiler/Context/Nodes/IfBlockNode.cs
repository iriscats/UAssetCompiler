using UAssetAPI.Kismet.Bytecode;

namespace UAssetCompiler.Decompiler.Context.Nodes;

public class IfBlockNode : BlockNode
{
    public KismetExpression Condition { get; set; }
}
