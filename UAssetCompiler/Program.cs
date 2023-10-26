using System.IO;
using UAssetAPI.UnrealTypes;
using UAssetAPI;
using UAssetCompiler;
using UAssetCompiler.Decompiler;
using UAssetCompiler.Json;

void FileProcessMain()
{
    var filePath = args[0];
    string fileExtension = Path.GetExtension(filePath);

    switch (fileExtension)
    {
        case ".json":
            var savePath = Path.ChangeExtension(filePath, "uasset");
            UAssetBinaryLinker.CreateUAsset(filePath, savePath);
            break;
        case ".uasset":
            UAssetBinaryLinker.LoadAsset(filePath);
            break;
        case ".pak":
            UAssetPacker.Unpack(filePath);
            break;
        default:
            Console.WriteLine(@"Input cmd error!");
            break;
    }
}

Console.WriteLine(@"Unreal Asset Compiler Tool v0.0.1");
Console.WriteLine(@"=================================");

if (args.Length > 0)
{
    var cmd = args[0];
    var mgr = new UAssetProjectManager();
    switch (cmd)
    {
        case "new":
            mgr.CreateProjectTpl(Environment.CurrentDirectory);
            break;
        case "start":
            string projectFile = Path.Combine(Environment.CurrentDirectory, "project.json");
            mgr.Run(projectFile);
            break;
        case "-f":
            mgr.Run(args[1]);
            break;
        case "-u":
            UAssetPacker.Unpack(args[1]);
            break;
        default:
            FileProcessMain();
            break;
    }
}

//Console.WriteLine("Press any key to exit...");
//Console.ReadKey();

var path =
    "/Users/bytedance/Project/kismet/UAssetCompiler/UAssetCompiler/data/Autocannon/Overclocks/OC_Autocannon_Neurotoxin_U.uasset";

//var gen = new UAssetScriptGenerator();
//Console.WriteLine(gen.MakeScript(path));

var gen = new UAssetJsonGenerator(path);
var json = gen.SerializeJson();
File.WriteAllText(path + ".json", json);