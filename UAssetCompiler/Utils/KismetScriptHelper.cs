namespace UAssetCompiler.Utils
{
    public class KismetScriptParser
    {
        private static HashSet<string> _additionalKeywords = new()
        {
            "bool",
            "byte",
            "int",
            "float",
            "string",
            "void",
            "true",
            "false"
        };

        public static bool IsKeyword(string keyword)
        {
            return _additionalKeywords.Contains(keyword);
        }
    }
}
