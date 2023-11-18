var curPath = Environment.CurrentDirectory;
var pathStr = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine);

Console.WriteLine("Starting set System Path...");

if (!pathStr!.Contains(curPath))
{
    Environment.SetEnvironmentVariable("Path", pathStr + ";" + curPath, EnvironmentVariableTarget.Machine);
    Console.WriteLine("Path set success！");
}
else
{
    Console.WriteLine("Path already set！");
}

Console.WriteLine("Do you need to add it to the system's context menu? Y/N");
var result = Console.ReadLine();
if (result! == "Y" || result! == "y")
{
    Console.WriteLine("Set system's context menu success!");
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();