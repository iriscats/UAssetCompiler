using System.Diagnostics;
using System.Text;

namespace UAssetCompiler.Utils
{
    internal static class ProcessUtils
    {
        public static string RunCmd(string exe, string arguments)
        {
            StringBuilder builder = new StringBuilder();
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = exe;

                //执行命令
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;//不启用shell
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;//不使用窗口
                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.OutputDataReceived += ProcessOutDataReceived;
                process.ErrorDataReceived += ProcessErrorReceived;

                process.WaitForExit();
                process.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

            return builder.ToString();
        }
        
        private static void ProcessOutDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private static void ProcessErrorReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }
        
        public static string RunExe(string exe, string arguments)
        {
            StringBuilder builder = new StringBuilder();
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = exe,
                    Arguments = arguments
                };
                Process.Start(startInfo);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

            return builder.ToString();
        }

    }
}



