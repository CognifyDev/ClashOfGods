using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace COG.Utils;

public static class PathUtils
{
    public static string? GetAmongUsPath(string additionPath = "")
    {
        var processes = Process.GetProcessesByName("Among Us");
        if (processes.Length == 0)
        {
            Main.Logger.LogError("Could not find the Among Us Process!”Among Us");
            return null;
        }

        try
        {
            var process = processes[0];
            if (process.MainModule == null)
            {
                Main.Logger.LogError("Could not get the main module of the Among Us process");
                return null;
            }
            
            var exePath = process.MainModule.FileName;
            var exeDir = Path.GetDirectoryName(exePath);
            if (exeDir == null)
            {
                Main.Logger.LogError("Could not get the directory name from the Among Us executable path");
                return null;
            }
            
            var storageDir = Path.Combine(exeDir, additionPath);
            return storageDir;
        }
        catch (System.Exception e)
        {
            Main.Logger.LogError($"Error getting Among Us path: {e.Message}");
            return null;
        }
    }
    public static void CopyResourcesToDisk(string targetDirectory, string embeddedDirectory, bool overwrite = true)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var FilePatch = $"COG.Resources.{embeddedDirectory}";

        foreach (var resourceName in assembly.GetManifestResourceNames())
        {
            if (resourceName.StartsWith(FilePatch))
            {
                var relativePath = resourceName.Substring(FilePatch.Length);
                var targetPath = Path.Combine(targetDirectory, relativePath);
                var directoryPath = Path.GetDirectoryName(targetPath);
                
                if (directoryPath != null)
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
                using (var fileStream = File.Create(targetPath))
                {
                    resourceStream?.CopyTo(fileStream);
                }

                Main.Logger.LogInfo($"File has Copied To : {relativePath} -> {targetPath}");
            }
        }
    }
}