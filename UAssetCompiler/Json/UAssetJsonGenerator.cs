using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.UnrealTypes;

namespace UAssetCompiler.Json
{
    internal class UAssetJsonGenerator
    {
        private readonly UAsset _asset;

        public UAssetJsonGenerator(string path)
        {
            _asset = new UAsset(path, EngineVersion.VER_UE4_27);
        }

        
        
        private List<string> MakeImports()
        {
            var imports = new List<string>();
    
            foreach (var import in _asset.Imports)
            {
                _writer.Write($@"import ""{import.ObjectName}""(""{import.ClassPackage}/{import.ClassName}"")");

                var outerIndex = import.OuterIndex;
                if (outerIndex.Index != 0)
                {
                    var outerImport = GetImport(outerIndex.Index);
                    _writer.Write($@" from ""{outerImport!.ObjectName}""");
                }
                else
                {
                    _writer.Write(@" from ""Package""");
                }

                _writer.WriteLine(@";");
            }
            
            return imports;
        }
        
        public string SerializeJson()
        {
            var doc = new UAssetJsonDocument(_asset)
            {
                Package = "",
            };
            doc.Imports.Add("");

            foreach (var item in _asset.Exports)
            {
                switch (item)
                {
                    case NormalExport normalExport:
                        doc.Exports.Add(new UacNormalExport(normalExport));
                        break;
                    default:
                        break;
                }
            }

            return JsonConvert.SerializeObject(doc, Formatting.Indented, UnrealPackage.jsonSettings);
        }
    }
}