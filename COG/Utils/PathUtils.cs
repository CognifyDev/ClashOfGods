using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace COG.Utils
{
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

            var exePath = processes[0].MainModule.FileName;
            var exeDir = Path.GetDirectoryName(exePath);
            var storageDir = Path.Combine(exeDir, additionPath);
            return storageDir;
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

                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

                    using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
                    using (var fileStream = File.Create(targetPath))
                    {
                        resourceStream.CopyTo(fileStream);
                    }

                    Main.Logger.LogInfo($"File has Copied To : {relativePath} -> {targetPath}");
                }
            }
        }
    }
}
