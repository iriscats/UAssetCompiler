using UAssetAPI.UnrealTypes;
using UAssetAPI;

namespace UAssetCompiler
{
    internal static class UAssetBinaryLinker
    {

        public static string CreateUAsset(string filePath, string destPath)
        {
            filePath = Path.GetFullPath(filePath);

            try
            {
                var json = File.ReadAllText(filePath);
                UAsset targetAsset = UAsset.DeserializeJson(json);
                targetAsset.Write(destPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }

            return destPath;
        }

        public static string LoadAsset(string filePath, EngineVersion version = EngineVersion.VER_UE4_27)
        {
            filePath = Path.GetFullPath(filePath);

            UAsset targetAsset = new UAsset();
            try
            {
                targetAsset = new UAsset(filePath, version);
                targetAsset.VerifyBinaryEquality();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            var json = targetAsset.SerializeJson(true);
            var savePath = Path.ChangeExtension(filePath, "json");
            File.WriteAllText(savePath, json);

            return savePath;
        }

    }
}
