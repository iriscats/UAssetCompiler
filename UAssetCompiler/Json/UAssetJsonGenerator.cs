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

        private UAsset _uAsset;

        public UAssetJsonGenerator(string path)
        {
            _uAsset = new UAsset(path, EngineVersion.VER_UE4_27);
        }


        public string SerializeJson()
        {

            var doc = new UAssetJsonDocument();
            doc.Package = "";
            doc.PackageSource = _uAsset.PackageSource;
            doc.PackageGuid = _uAsset.PackageGuid;
            doc.Imports.Add("");

            foreach (var item in _uAsset.Exports)
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
