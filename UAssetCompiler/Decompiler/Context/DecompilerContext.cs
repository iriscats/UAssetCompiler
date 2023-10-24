using UAssetAPI;
using UAssetAPI.ExportTypes;

namespace UAssetCompiler.Decompiler.Context
{
    public class DecompilerContext
    {
        public UnrealPackage Asset { get; init; }
        public ClassExport Class { get; init; }
        public FunctionExport Function { get; init; }

        public DecompilerContext()
        {
        }
    }
}