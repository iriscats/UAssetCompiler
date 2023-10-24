

var curPath = Environment.CurrentDirectory;
var pathStr = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine);

if (!pathStr!.Contains(curPath))
{
    Environment.SetEnvironmentVariable("Path", pathStr + ";" + curPath, EnvironmentVariableTarget.Machine);
    Console.WriteLine("Path set success！");
}
else {
    Console.WriteLine("Path already set！");
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();