using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.JSON;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.UnrealTypes;
using UAssetCompiler.Json.Converter;

namespace UAssetCompiler.Json
{
    internal class UAssetJsonGenerator
    {
        private UAsset _asset;
        private readonly UAssetJsonDocument _doc;
        private ImportConverter? _importConverter;

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

        private UacExport? GetExport(int index) => index < 1 ? null : _doc.Exports[index - 1];

        public int TokenToIndex(string token)
        {
            var tokenArr = token.Split(":");
            if (tokenArr.Length < 2)
            {
                return 0;
            }

            if (tokenArr[0] == "Import")
            {
                return _asset.SearchForImport(new FName(_asset, token));
            }

            if (tokenArr[0] == "Export")
            {
                return _asset.GetExportIndex(tokenArr[1]);
            }

            throw new NotImplementedException();
        }

        public string IndexToToken(int index)
        {
            if (index < 0)
            {
                var import = _asset.GetImport(index);
                return "Import:" + import!.ObjectName;
            }

            if (index > 0)
            {
                return "Export:" + GetExport(index)!.ObjectName;
            }

            return "Object";
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
            var exportDic = new Dictionary<string, Export>();
            for (var i = 0; i < _asset.Exports.Count; i++)
            {
                var export = _asset.Exports[i];
                var name = export.ObjectName.ToString()!;
                if (exportDic.ContainsKey(name))
                {
                    name += $"_{i}";
                }

                exportDic[name] = export;
                switch (export)
                {
                    case NormalExport normalExport:
                    {
                        var uacExport = new UacNormalExport(normalExport)
                        {
                            ObjectName = name
                        };
                        _doc.Exports.Add(uacExport);
                    }
                        break;
                    default:
                        break;
                }
            }

            for (var i = 0; i < _asset.Exports.Count; i++)
            {
                var export = _asset.Exports[i];
                var uacExport = _doc.Exports[i];

                uacExport.OuterObject = IndexToToken(export.OuterIndex.Index);
                uacExport.SuperObject = IndexToToken(export.SuperIndex.Index);
                uacExport.Class = IndexToToken(export.ClassIndex.Index);
                uacExport.TemplateObject = IndexToToken(export.TemplateIndex.Index);
                uacExport.ObjectFlags = export.ObjectFlags.ToString();
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

        private List<FPackageIndex> CollectAllDepends(UacExport export)
        {
            var list = new List<FPackageIndex>();
            if (export is UacNormalExport normalExport)
            {
                foreach (var propertyData in normalExport.Data)
                {
                    switch (propertyData)
                    {
                        case ObjectPropertyData objectPropertyData:
                            list.Add(objectPropertyData.Value);
                            break;
                        case MapPropertyData mapPropertyData:
                            foreach (var mapItem
                                     in mapPropertyData.Value)
                            {
                                if (mapItem.Key is ObjectPropertyData mapKeyObjectPropertyData)
                                {
                                    list.Add(mapKeyObjectPropertyData.Value);
                                }

                                if (mapItem.Value is ObjectPropertyData mapValueObjectPropertyData)
                                {
                                    list.Add(mapValueObjectPropertyData.Value);
                                }
                            }

                            break;
                    }
                }
            }

            return list;
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
            var doc = JsonConvert.DeserializeObject<UAssetJsonDocument>(json);
            _asset = new UAsset(EngineVersion.VER_UE4_27)
            {
                PackageSource = doc!.PackageSource,
                PackageGuid = doc.PackageGuid
            };
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
                var origin = doc.Exports[i] as UacNormalExport;

                var dest = new NormalExport
                {
                    ObjectName = new FName(_asset, origin!.ObjectName),
                    Data = origin.Data,
                    bNotAlwaysLoadedForEditorGame = true,
                    OuterIndex = FPackageIndex.FromRawIndex(TokenToIndex(origin.OuterObject)),
                    SuperIndex = FPackageIndex.FromRawIndex(TokenToIndex(origin.SuperObject)),
                    TemplateIndex = FPackageIndex.FromRawIndex(TokenToIndex(origin.TemplateObject)),
                    ClassIndex = FPackageIndex.FromRawIndex(TokenToIndex(origin.Class)),
                    Extras = new byte[] { 0x00, 0x00, 0x00, 0x00 },
                };

                if (i == 0)
                {
                    dest.bIsAsset = true;
                }

                if (Enum.TryParse(origin.ObjectFlags, out EObjectFlags flags))
                {
                    dest.ObjectFlags = flags;
                }

                dest.CreateBeforeSerializationDependencies = new List<FPackageIndex>();
                dest.CreateBeforeSerializationDependencies.AddRange(CollectAllDepends(origin));
                dest.SerializationBeforeCreateDependencies =
                    new List<FPackageIndex> { dest.ClassIndex, dest.TemplateIndex };

                dest.CreateBeforeCreateDependencies = new List<FPackageIndex>();
                if (dest.OuterIndex.Index != 0)
                {
                    dest.CreateBeforeCreateDependencies.Add(dest.OuterIndex);
                }

                _asset.Exports[i] = dest;
            }
            
            _asset.MakeDependsMap();
            _asset.MakeGenerations();
            _asset.CountExportData();

            return Asset;
        }
    }
}