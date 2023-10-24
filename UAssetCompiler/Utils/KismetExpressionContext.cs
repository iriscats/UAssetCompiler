using UAssetAPI.Kismet.Bytecode;

namespace UAssetCompiler.Utils;

public record KismetExpressionContext<T>(
    KismetExpression Expression,
    int CodeStartOffset,
    T Tag)
{
    public int? CodeEndOffset { get; set; }
}
