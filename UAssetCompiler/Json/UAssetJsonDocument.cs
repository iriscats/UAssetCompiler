using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAssetAPI.ExportTypes;
using UAssetAPI.FieldTypes;
using UAssetAPI.Kismet.Bytecode;
using UAssetAPI.PropertyTypes.Objects;

namespace UAssetCompiler.Json
{
    class UAssetJsonDocument
    {
        public string Package;

        public Guid PackageGuid;

        public uint PackageSource;

        public int UacVersion = 1;

        public List<string> Imports = new List<string>();

        public List<UacExport> Exports = new List<UacExport>();

    }


    class UacExport
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


    class UacFuntionExport : UacExport
    {
        public UacFuntionExport(FunctionExport functionExport) : base(functionExport)
        {
            LoadedProperties = functionExport.LoadedProperties.ToList();
            ScriptBytecode = functionExport.ScriptBytecode.ToList();
        }
        public List<FProperty> LoadedProperties = new List<FProperty>();

        public List<KismetExpression> ScriptBytecode = new List<KismetExpression>();
    }

    class UacNormalExport : UacExport
    {
        public UacNormalExport(NormalExport normalExport) : base(normalExport)
        {
            Data = normalExport.Data;
        }

        public List<PropertyData> Data = new List<PropertyData>();

    }




}
