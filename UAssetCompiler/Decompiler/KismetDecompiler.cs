using UAssetAPI;

namespace UAssetCompiler.Decompiler;

public partial class KismetDecompiler
{
    private readonly UnrealPackage _asset;
    private readonly IndentedWriter _writer;

    public KismetDecompiler(IndentedWriter writer, UnrealPackage asset)
    {
        _writer = writer;
        _asset = asset;
    }
}