using UAssetAPI.ExportTypes;

namespace UAssetCompiler.Decompiler;

public partial class KismetDecompiler
{
    public void WriteExports()
    {
        if (_asset.GetClassExport() is { } classExport)
        {
            WriteFunctionClass(classExport);
            return;
        }

        foreach (var export in _asset.Exports)
        {
            switch (export)
            {
                case FunctionExport functionExport:
                    DecompileFunction(functionExport);
                    break;
                case NormalExport normalExport:
                    
                    break;
            }

            Console.WriteLine(export.GetType());
        }
    }
}