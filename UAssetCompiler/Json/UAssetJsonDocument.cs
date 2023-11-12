using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.FieldTypes;
using UAssetAPI.Kismet.Bytecode;
using UAssetAPI.PropertyTypes.Objects;

namespace UAssetCompiler.Json
{
    public class UAssetJsonDocument
    {
        public UAssetJsonDocument()
        {
        }

        public UAssetJsonDocument(UAsset asset)
        {
            PackageGuid = asset.PackageGuid;
            PackageSource = asset.PackageSource;
        }

        public string Package = "";

        public Guid PackageGuid;

        public uint PackageSource;

        public int UacVersion = 1;

        public List<string> Imports = new();

        public List<UacExport> Exports = new();
    }


    public class UacExport
    {
        public string ObjectName;

        public string OuterObject;

        public string Class;

        public string SuperObject;

        public string TemplateObject;

        public string ObjectFlags;
    }


    public class UacFunctionExport : UacExport
    {
        public UacFunctionExport()
        {
        }

        public UacFunctionExport(FunctionExport functionExport) 
        {
            LoadedProperties = functionExport.LoadedProperties.ToList();
            ScriptBytecode = functionExport.ScriptBytecode.ToList();
        }

        public List<FProperty> LoadedProperties;

        public List<KismetExpression> ScriptBytecode;
    }

    public class UacNormalExport : UacExport
    {
        public UacNormalExport()
        {
        }
        
        public UacNormalExport(NormalExport normalExport)
        {
            Data = normalExport.Data;
        }

        public List<PropertyData> Data;
    }
}