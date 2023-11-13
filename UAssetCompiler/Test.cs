﻿using UAssetCompiler.Decompiler;

namespace UAssetCompiler
{
    internal class Test
    {

        string[] TestProjectFile()
        {
            var arr = new List<string>();
            arr.Add("-f");
            arr.Add("C:\\Users\\11297\\Desktop\\test\\project.json");
            return arr.ToArray();
        }

        string[] TestUnpack()
        {
            var arr = new List<string>();
            arr.Add("-u");
            arr.Add("C:\\Users\\11297\\Desktop\\FSD-WindowsNoEditor.pak");
            return arr.ToArray();
        }

        void TestUAssetScriptGenerator()
        {
            var gen = new UAssetScriptGenerator("/Users/bytedance/Project/UAssetCompiler/UAssetCompiler/data/Shooter/BP_Mactera_Heavy_ProjectileAttack.uasset");
            Console.WriteLine(gen.MakeScript());
        }

        public static string[] TestSingleFile()
        {
            var arr = new List<string>();
            arr.Add(@"D:\mod\DRGPacker4.27\FSD-WindowsNoEditor\FSD\Content\WeaponsNTools\Autocannon\Overclocks\OSB_Autocannon.uasset.json");
            return arr.ToArray();
        }

    }
}
