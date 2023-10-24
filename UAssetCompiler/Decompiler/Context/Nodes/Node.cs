using UAssetAPI.Kismet.Bytecode;

namespace UAssetCompiler.Decompiler.Context.Nodes;

public class Node
{
    public Node Parent { get; set; }
    public KismetExpression Source { get; set; }
    public int CodeStartOffset { get; set; }
    public int CodeEndOffset { get; set; }
    public HashSet<Node> ReferencedBy { get; init; } = new();
    public List<Node> Children { get; init; } = new();

    public Node()
    {
    }

    public override string ToString()
    {
        return $"{CodeStartOffset}: {Source.Inst} {string.Join(' ', Children.Select(x => x.ToString()))}";
    }
}