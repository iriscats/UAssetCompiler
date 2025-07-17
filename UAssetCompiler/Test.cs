using UAssetCompiler.Decompiler;

namespace UAssetCompiler
{
    internal class Test
    {
        string[] TestProjectFile()
        {
            var arr = new List<string>();
            arr.Add("-f");
            arr.Add("C:\\Users\\11297\\Desktop\\test\\project.json");
            return arr.ToArray();
        }

        string[] TestUnpack()
        {
            var arr = new List<string>();
            arr.Add("-u");
            arr.Add("C:\\Users\\11297\\Desktop\\FSD-WindowsNoEditor.pak");
            return arr.ToArray();
        }

        public static void TestUAssetScriptGenerator()
        {
            var gen = new UAssetScriptGenerator("/Users/bytedance/Project/kismet/sandbox/Hooky/HookyLib.uasset");
            Console.WriteLine(gen.MakeScript());
        }

        public static string[] TestSingleFile()
        {
            var arr = new List<string>();
            arr.Add("-d");
            arr.Add(@"/Users/bytedance/Project/kismet/sandbox/Hooky/HookyLib.uasset");
            return arr.ToArray();
        }
    }
}