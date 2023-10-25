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
        public UAssetJsonDocument(UAsset asset)
        {
            PackageGuid = asset.PackageGuid;
            PackageSource = asset.PackageSource;
        }

        public string Package = "";

        public Guid PackageGuid;

        public uint PackageSource;

        public int UacVersion = 1;

        public List<string> Imports = new List<string>();

        public List<UacExport> Exports = new List<UacExport>();
    }


    public class UacExport
    {
        public UacExport(Export export)
        {
            ObjectName = export.ObjectName.ToString();
            OuterIndex = export.OuterIndex.Index;
            ClassIndex = export.ClassIndex.Index;
            SuperIndex = export.SuperIndex.Index;
            TemplateIndex = export.TemplateIndex.Index;
        }

        public string ObjectName;

        public int OuterIndex = 0;

        public int ClassIndex = 0;

        public int SuperIndex = 0;

        public int TemplateIndex = 0;
    }


    public class UacFunctionExport : UacExport
    {
        public UacFunctionExport(FunctionExport functionExport) : base(functionExport)
        {
            LoadedProperties = functionExport.LoadedProperties.ToList();
            ScriptBytecode = functionExport.ScriptBytecode.ToList();
        }

        public List<FProperty> LoadedProperties;

        public List<KismetExpression> ScriptBytecode;
    }

    public class UacNormalExport : UacExport
    {
        public UacNormalExport(NormalExport normalExport) : base(normalExport)
        {
            Data = normalExport.Data;
        }

        public List<PropertyData> Data;
    }
}