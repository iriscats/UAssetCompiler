using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.JSON;
using UAssetAPI.UnrealTypes;

namespace UAssetCompiler.Json
{
    public class UAssetJsonGenerator
    {
        private UAsset _asset;
        private readonly UAssetJsonDocument _doc;
        private ImportConverter? _importConverter;
        private ExportConverter? _exportConverter;

        public UAsset Asset => _asset;

        public UAssetJsonGenerator()
        {
            _asset = new UAsset(EngineVersion.VER_UE4_27);
            _doc = new UAssetJsonDocument();
        }

        public UAssetJsonGenerator(string path)
        {
            _asset = new UAsset(path, EngineVersion.VER_UE4_27);
            _doc = new UAssetJsonDocument(_asset);
        }

        public int TokenToIndex(string token)
        {
            var tokenArr = token.Split(":");
            if (tokenArr.Length < 2)
            {
                return 0;
            }

            return tokenArr[0] switch
            {
                "Import" => _asset.SearchForImport(new FName(_asset, token)),
                "Export" => _asset.GetExportIndex(tokenArr[1]),
                _ => throw new NotImplementedException()
            };
        }

        public string IndexToToken(int index)
        {
            return index switch
            {
                < 0 => "Import:" + _asset.GetImport(index)!.ObjectName,
                > 0 => "Export:" + _asset.GetExport(index)!.ObjectName,
                _ => "Object"
            };
        }

        private void ImportToJson()
        {
            _importConverter = new ImportConverter(_asset);
            var imports = new List<string>();
            foreach (var import in _asset.Imports)
            {
                if (import.OuterIndex.Index == 0)
                {
                    continue;
                }

                imports.Add(_importConverter.ImportToToken(import));
            }

            _doc.Imports = imports;
        }

        private void ExportToJson()
        {
            _exportConverter = new ExportConverter(_asset, this);
            _exportConverter.SolvingDuplicateObjectName();
            foreach (var export in _asset.Exports)
            {
                var uacExport = _exportConverter.ToUacExport(export);
                _doc.Exports.Add(uacExport);
            }
        }

        private void MakeImports(UAssetJsonDocument doc)
        {
            _importConverter = new ImportConverter(_asset);
            _asset.Imports = new List<Import>();

            foreach (var import in doc.Imports)
            {
                _importConverter.TokenToImports(import);
            }
        }

        private void PrepareExports(UAssetJsonDocument doc)
        {
            _asset.Exports = new List<Export>();
            foreach (var export in doc.Exports)
            {
                _asset.Exports.Add(new NormalExport(_asset, null)
                {
                    ObjectName = new FName(_asset, export.ObjectName)
                });
            }
        }

        public string SerializeJson()
        {
            ImportToJson();
            ExportToJson();

            var jsonSettings = new JsonSerializerSettings
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
            var doc = JsonConvert.DeserializeObject<UAssetJsonDocument>(json);
            _asset = new UAsset(EngineVersion.VER_UE4_27)
            {
                PackageSource = doc!.PackageSource,
                PackageGuid = doc.PackageGuid
            };
            _exportConverter = new ExportConverter(_asset, this);
            _asset.MakeUAssetHeader();
            _asset.AddNameReference(doc.Package);

            MakeImports(doc);
            PrepareExports(doc);

            doc = JsonConvert.DeserializeObject<UAssetJsonDocument>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                NullValueHandling = NullValueHandling.Include,
                FloatParseHandling = FloatParseHandling.Double,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new MyUAssetContractResolver(_asset),
                Converters = new List<JsonConverter>()
                {
                    new FSignedZeroJsonConverter(),
                    new FStringTableJsonConverter(),
                    new FStringJsonConverter(),
                    new FPackageIndexJsonConverter(),
                    new StringEnumConverter(),
                    new PropertyDataConverter(this, true)
                }
            });

            for (var i = 0; i < doc!.Exports.Count; i++)
            {
                _asset.Exports[i] = _exportConverter.ToExport(doc.Exports[i]);
            }

            _asset.Exports[0].bIsAsset = true;
            
            _asset.MakeDependsMap();
            _asset.MakeGenerations();
            _asset.CountExportData();

            return Asset;
        }
    }
}