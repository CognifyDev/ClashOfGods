using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using Mono.Cecil.Cil;

namespace COG.Utils
{
    public static class PathUtils
    {
        public static string GetAmongUsPath(string additionPath = "")
        {
            Process[] processes = Process.GetProcessesByName("Among Us");
            if (processes.Length == 0)
            {
                Main.Logger.LogError("Could not find the Among Us Process!”Among Us");
                return null;
            }

            string exePath = processes[0].MainModule.FileName;
            string exeDir = Path.GetDirectoryName(exePath);
            string storageDir = Path.Combine(exeDir, additionPath);
            return storageDir;
        }
        public static void CopyResourcesToDisk(string targetDirectory, string embeddedDirectory, bool overwrite = true)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string FilePatch = $"COG.Resources.{embeddedDirectory}";

            foreach (string resourceName in assembly.GetManifestResourceNames())
            {
                if (resourceName.StartsWith(FilePatch))
                {
                    string relativePath = resourceName.Substring(FilePatch.Length);
                    string targetPath = Path.Combine(targetDirectory, relativePath);

                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

                    using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
                    using (FileStream fileStream = File.Create(targetPath))
                    {
                        resourceStream.CopyTo(fileStream);
                    }

                    Main.Logger.LogInfo($"File has Copied To : {relativePath} -> {targetPath}");
                }
            }
        }
    }
}
