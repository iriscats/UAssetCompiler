using Newtonsoft.Json;
using System.Text;
using UAssetCompiler.Utils;

namespace UAssetCompiler
{
    class PathOption
    {
        public string Source = "";

        public string Target = "";
    }


    class DevOption
    {
        public bool IsOpenGame = false;

        public string GamePath = "";
    }

    class UAssetProjectBuilder
    {
        public string Name = "";

        public string Author = "";

        public string Version = "1.0.0";

        public string SdkVer = "0.0.1";

        public string? Description = "";

        public List<PathOption> Files = new List<PathOption>();

        public DevOption Dev = new DevOption();
    }


    class UAssetProjectManager
    {
        private string _projectPath;

        private string ProjectBuildPath => Path.Combine(this._projectPath, "build");

        private UAssetProjectBuilder? _dataSource = null;

        public UAssetProjectManager()
        {
        }

        private void CleanTemp()
        {
            if (!Directory.Exists(ProjectBuildPath))
                return;

            Directory.Delete(this.ProjectBuildPath, true);
            Console.WriteLine(@"Clean Temp Directory Success! ：" + ProjectBuildPath);
        }

        private void BuildModDirectory()
        {
            foreach (var item in this._dataSource!.Files)
            {
                string destFilePath = Path.Combine(this.ProjectBuildPath, item.Target);
                string destPath = Path.GetDirectoryName(destFilePath);
                if (destPath == null)
                {
                    throw new Exception($"{destFilePath} is incorrect");
                }

                Directory.CreateDirectory(destPath);

                string ext = Path.GetExtension(item.Source);
                string srcFilePath = item.Source;
                Console.WriteLine("Process: "+ srcFilePath);

                if (ext == ".json")
                {
                    UAssetBinaryLinker.CreateUAsset(Path.Combine(this._projectPath, item.Source), destFilePath);
                }

                if (ext == ".locres")
                {
                    File.Copy(srcFilePath, destFilePath);
                }

                if (ext == ".uasset")
                {
                    File.Copy(srcFilePath, destFilePath);
                    srcFilePath = Path.ChangeExtension(srcFilePath, "uexp");
                    destFilePath = Path.ChangeExtension(destFilePath, "uexp");
                    File.Copy(srcFilePath, destFilePath);
                }
            }
        }

        private void CreateCopyright()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Author: {this._dataSource!.Author}");
            builder.AppendLine($"Version: {this._dataSource!.Version}");
            builder.AppendLine($"Description: {this._dataSource!.Description}");
            File.WriteAllText(Path.Combine(this.ProjectBuildPath, "Copyright.txt"), builder.ToString());
        }


        public void Run(string projectFilePath)
        {
            projectFilePath = Path.GetFullPath(projectFilePath);
            _projectPath = Path.GetDirectoryName(projectFilePath);

            string json = File.ReadAllText(projectFilePath);
            json = json.Replace("\\", "\\\\");
            _dataSource = JsonConvert.DeserializeObject<UAssetProjectBuilder>(json);

            CleanTemp();
            BuildModDirectory();
            CreateCopyright();

            UAssetPacker.Pack(ProjectBuildPath, _dataSource!.Name, _dataSource!.Dev.GamePath);

            if (_dataSource!.Dev.IsOpenGame)
            {
                ProcessUtils.RunExe("explorer.exe", "steam://rungameid/548430");
            }

            Console.WriteLine(@"Create Pak Success!");
        }


        public void CreateProjectTpl(string path)
        {
            var project = new UAssetProjectBuilder();
            project.Files.Add(new PathOption());
            var json = JsonConvert.SerializeObject(project, Formatting.Indented);
            var jsonProject = Path.Combine(path, "project.json");
            File.WriteAllText(jsonProject, json);
            Console.WriteLine(@$"Create Success! ：{jsonProject} ");
        }
    }
}