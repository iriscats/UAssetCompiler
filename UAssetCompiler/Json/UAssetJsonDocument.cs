using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAssetAPI.ExportTypes;

namespace UAssetCompiler.Json
{
    class UAssetJsonDocument
    {
        public string Package;

        public Guid PackageGuid;

        public uint PackageSource;

        public int UacVersion = 1;

        public List<string> Imports = new List<string>();

        public List<Export> Exports = new List<Export>();

    }


    class UacExport
    {
        public string ObjectName;

        public uint OuterIndex = 0;

        public uint ClassIndex = 0;

        public uint SuperIndex = 0;

        public uint TemplateIndex = 0;
    }


    class UacFuntionExport : UacExport
    {
        public List<string> LoadedProperties = new List<string>();

        public List<string> ScriptBytecode = new List<string>();
    }

    class UacNormalExport : UacExport
    {
        public List<string> Data = new List<string>();

    }




}
