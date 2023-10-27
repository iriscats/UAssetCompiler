using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.JSON;
using UAssetAPI.UnrealTypes;

namespace UAssetCompiler.Json
{
    internal class UAssetJsonGenerator
    {
        private readonly UAsset _asset;
        private readonly UAssetJsonDocument _doc;

        public UAssetJsonGenerator(string path)
        {
            _asset = new UAsset(path, EngineVersion.VER_UE4_27);

            _doc = new UAssetJsonDocument(_asset)
            {
                Package = "",
            };
        }

        private Import? GetImport(int index) => index > -1 ? null : _asset.Imports[index * -1 - 1];
        private UacExport? GetExport(int index) => index < 1 ? null : _doc.Exports[index - 1];


        public string IndexToToken(int index)
        {
            if (index < 0)
            {
                var import = GetImport(index);
                return "[Import]" + import!.ObjectName.ToString();
            }
            else if (index > 0)
            {
                return "[Export]" + GetExport(index)!.ObjectName.ToString();
            }

            return "Object";
        }


        private string MakeImportToken(Import import)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"{import.ObjectName}({import.ClassName})");

            var outerIndex = import.OuterIndex;
            var outerImport = GetImport(outerIndex.Index);
            builder.Append($"->{outerImport!.ObjectName}");

            return builder.ToString();
        }


        private void MakeImports()
        {
            var imports = new List<string>();

            foreach (var import in _asset.Imports)
            {
                if (import.OuterIndex.Index == 0)
                {
                    continue;
                }

                imports.Add(MakeImportToken(import));
            }

            _doc.Imports = imports;
        }


        private void MakeExports()
        {
            var exportDic = new Dictionary<string, Export>();
            for (int i = 0; i < _asset.Exports.Count; i++)
            {
                var export = _asset.Exports[i];
                var name = export.ObjectName.ToString();
                if (exportDic.ContainsKey(name))
                {
                    name += $"_{i}";
                }

                exportDic[name] = export;

                switch (export)
                {
                    case NormalExport normalExport:
                    {
                        var uacExport = new UacNormalExport(normalExport);
                        uacExport.ObjectName = name;
                        _doc.Exports.Add(uacExport);
                    }
                        break;
                    default:
                        break;
                }
            }

            for (int i = 0; i < _asset.Exports.Count; i++)
            {
                var export = _asset.Exports[i];
                var uacExport = _doc.Exports[i];

                uacExport.OuterObject = IndexToToken(export.OuterIndex.Index);
                uacExport.SuperObject = IndexToToken(export.SuperIndex.Index);
                uacExport.Class = IndexToToken(export.ClassIndex.Index);
                uacExport.TemplateObject = IndexToToken(export.TemplateIndex.Index);
            }
        }

        public string SerializeJson()
        {
            MakeImports();
            MakeExports();

            JsonSerializerSettings jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                NullValueHandling = NullValueHandling.Ignore,
                FloatParseHandling = FloatParseHandling.Double,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new UAssetContractResolver(null),
                Converters = new List<JsonConverter>
                {
                    new FSignedZeroJsonConverter(),
                    new FNameJsonConverter(null),
                    new FStringTableJsonConverter(),
                    new FStringJsonConverter(),
                    new FPackageIndexJsonConverter(),
                    new StringEnumConverter(),
                    new GuidJsonConverter(),
                    new ObjectPropertyDataConverter(this),
                    new IntPropertyDataConverter(),
                    new BoolPropertyDataConverter(),
                    new GuidPropertyDataConverter(),
                    new FloatPropertyDataConverter(),
                    new NamePropertyDataConverter()
                }
            };

            return JsonConvert.SerializeObject(_doc, Formatting.Indented, jsonSettings);
        }

        public static UAssetJsonDocument? FromJson(string json)
        {
            Dictionary<FName, string> toBeFilled = new Dictionary<FName, string>();
            return JsonConvert.DeserializeObject<UAssetJsonDocument>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                NullValueHandling = NullValueHandling.Include,
                FloatParseHandling = FloatParseHandling.Double,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new UAssetContractResolver(toBeFilled),
                Converters = new List<JsonConverter>()
                {
                    new FSignedZeroJsonConverter(),
                    new FNameJsonConverter(null),
                    new FStringTableJsonConverter(),
                    new FStringJsonConverter(),
                    new FPackageIndexJsonConverter(),
                    new StringEnumConverter(),
                    new GuidJsonConverter(),
                    new ObjectPropertyDataConverter(),
                    new IntPropertyDataConverter(),
                    new BoolPropertyDataConverter(),
                    new GuidPropertyDataConverter(),
                    new FloatPropertyDataConverter(),
                    new NamePropertyDataConverter()
                }
            });
        }
    }
}