using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.JSON;
using UAssetAPI.UnrealTypes;
using UAssetCompiler.Json.Converter;

namespace UAssetCompiler.Json
{
    internal class UAssetJsonGenerator
    {
        private UAsset _asset;
        private readonly UAssetJsonDocument _doc;

        public UAsset Asset => _asset;

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

        private int FindImport(string token)
        {
            return -1;
        }

        private int FindExport(string token)
        {
            return 1;
        }

        public int TokenToIndex(string token)
        {
            var tokenArr = token.Split(":");
            if (tokenArr.Length < 2)
            {
                return 0;
            }

            if (tokenArr[0] == "Import")
            {
                return FindImport(tokenArr[1]);
            }

            if (tokenArr[0] == "Export")
            {
                return FindExport(tokenArr[1]);
            }

            throw new NotImplementedException();
        }

        public string IndexToToken(int index)
        {
            if (index < 0)
            {
                var import = GetImport(index);
                return "Import:" + import!.ObjectName;
            }

            if (index > 0)
            {
                return "Export:" + GetExport(index)!.ObjectName;
            }

            return "Object";
        }

        private string ImportToToken(Import import)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"{import.ObjectName}({import.ClassName})");

            var outerIndex = import.OuterIndex;
            var outerImport = GetImport(outerIndex.Index);
            builder.Append($"->{outerImport!.ObjectName}");

            return builder.ToString();
        }

        private Import TokenToImport(string token)
        {
            
            
            return new Import(
                new FName(_asset, ""),
                new FName(_asset, ""),
                new FPackageIndex(0),
                new FName(_asset, ""),
                false
            );
        }

        private Export TokenToExport(UacExport token)
        {
            return new Export(_asset, null);
        }

        private void ImportToJson()
        {
            var imports = new List<string>();

            foreach (var import in _asset.Imports)
            {
                if (import.OuterIndex.Index == 0)
                {
                    continue;
                }

                imports.Add(ImportToToken(import));
            }

            _doc.Imports = imports;
        }

        private void ExportToJson()
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

        private void JsonToImport(string json)
        {
            var doc = JsonConvert.DeserializeObject<UAssetJsonDocument>(json);
            foreach (var import in doc!.Imports)
            {
                _asset.Imports.Add(TokenToImport(import));
            }
        }

        private void JsonToExport(string json)
        {
            var doc = JsonConvert.DeserializeObject<UAssetJsonDocument>(json);
            foreach (var export in doc!.Exports)
            {
                _asset.Exports.Add(TokenToExport(export));
            }
        }

        public string SerializeJson()
        {
            ImportToJson();
            ExportToJson();

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
                    new PropertyDataConverter(this, false),
                }
            };

            return JsonConvert.SerializeObject(_doc, Formatting.Indented, jsonSettings);
        }

        public UAsset FromJson(string json)
        {
            _asset = new UAsset(EngineVersion.VER_UE4_27);

            JsonToImport(json);
            JsonToExport(json);

            Dictionary<FName, string> toBeFilled = new Dictionary<FName, string>();
            var doc = JsonConvert.DeserializeObject<UAssetJsonDocument>(json, new JsonSerializerSettings
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
                    new PropertyDataConverter(this, true)
                }
            });

            return Asset;
        }
    }
}