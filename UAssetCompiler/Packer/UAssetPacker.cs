using System.Diagnostics;
using UAssetCompiler.Utils;

namespace UAssetCompiler
{
    public static class UAssetPacker
    {
        private static readonly string UnrealPakPath = Path.Combine(Process.GetCurrentProcess().MainModule.FileName, @"..\UnrealPak\Engine\Binaries\Win64\UnrealPak.exe");

        public static bool Pack(string resDirectory, string name, string destDirectory)
        {
            Console.WriteLine(@"UnrealPak pack start!");

            string txt = $"\"{resDirectory}\\*.*\" \"..\\..\\..\\FSD\\*.*\"";
            string txtPath = Path.Combine(resDirectory, "autogen.txt");
            File.WriteAllText(txtPath, txt);

            string pakPath = Path.Combine(destDirectory, $"FSD-WindowsNoEditor_{name}.pak");
            string cmd = $"\"{pakPath}\" -platform=\"Windows\" -create=\"{txtPath}\" -compress";

            try
            {
                Console.WriteLine(cmd);
                ProcessUtils.RunCmd(UnrealPakPath, cmd);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool Unpack(string pakPath)
        {
            Console.WriteLine(@"UnrealPak unpack start!");
            pakPath = Path.GetFullPath(pakPath);
     
            string dir = Path.GetDirectoryName(Path.GetFullPath(pakPath));
            string fileName = Path.GetFileNameWithoutExtension(pakPath);

            string cmd = $"\"{pakPath}\" -platform=\"Windows\" -extract \"{Path.Combine(dir, fileName)}\"";
            try
            {
                Console.WriteLine(cmd);
                ProcessUtils.RunCmd(UnrealPakPath, cmd);
            }
            catch
            {
                return false;
            }

            return true;
        }

    }
}

