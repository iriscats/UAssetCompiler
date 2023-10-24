using UAssetCompiler.Decompiler.Context;
using UAssetCompiler.Decompiler.Context.Nodes;

namespace UAssetCompiler.Decompiler.Passes
{
    public interface IDecompilerPass
    {
        Node Execute(DecompilerContext context, Node root);
    }
}
