using System.Reflection.Emit;
using UAssetCompiler;
using UAssetCompiler.Json;


//args = Test.TestSingleFile();

void FileProcessMain()
{
    var filePath = args[0];
    string fileExtension = Path.GetExtension(filePath);

    switch (fileExtension)
    {
        case ".json":

            if (filePath.EndsWith(".uasset.json"))
            {
                filePath = Path.GetFullPath(filePath);
                Console.WriteLine(filePath);
                var generator = new UAssetJsonGenerator();

                var json = File.ReadAllText(filePath);
                var targetAsset = generator.FromJson(json);
                targetAsset.Write(filePath.Replace(".uasset.json", ".uasset"));
            }
            else
            {
                var savePath = Path.ChangeExtension(filePath, "uasset");
                UAssetBinaryLinker.CreateUAsset(filePath, savePath);
            }

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
        case "-d":
            var generator = new UAssetJsonGenerator(args[1]);
            var json = generator.SerializeJson();
            File.WriteAllText(args[1] + ".json", json);
            break;
        default:
            FileProcessMain();
            break;
    }
}

//Console.WriteLine("Press any key to exit...");
//Console.ReadKey();


