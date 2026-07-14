#if WINDOWS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BepInEx;
using COG.Utils;

namespace COG.Asset.Dependence;

public static class DependenceDownloader
{
    private static readonly Dictionary<string, string> ExpectedHashes = new()
    {
        ["Acornima.dll"] = "",
        ["YamlDotNet.dll"] = "",
    };

    public static IEnumerator DownloadCommonDependence()
    {
        yield return AdvancedExampleCoroutine("https://github.com/CognifyDev/ClashOfGods/raw/refs/heads/main/Dependencies/Acornima.dll", Path.Combine(Paths.GameRootPath, "BepInEx", "core"));
    }

    public static IEnumerator DownloadYaml()
    {
        yield return AdvancedExampleCoroutine("https://github.com/CognifyDev/ClashOfGods/raw/refs/heads/main/Dependencies/YamlDotNet.dll", Path.Combine(Paths.GameRootPath, "BepInEx", "core"));
    }

    private static IEnumerator AdvancedExampleCoroutine(string targetFile, string targetPath)
    {
        var task = AdvancedExample(targetFile, targetPath);
        yield return WaitForTaskCompletion(task);
    }

    private static IEnumerator WaitForTaskCompletion(Task task)
    {
        while (!task.IsCompleted)
        {
            yield return null;
        }
    }

    private static async Task AdvancedExample(string targetFile, string targetPath)
    {
        var downloader = new AdvancedFileDownloader();

        downloader.ProgressChanged += (_, progress) =>
        {
            Main.Logger.LogInfo($"Downloading Progress: {progress:F1}%");
        };

        var success = await downloader.DownloadAndMoveAsync(targetFile, targetPath);

        if (success)
        {
            var fileName = Path.GetFileName(new Uri(targetFile).LocalPath);
            var filePath = Path.Combine(targetPath, fileName);
            VerifyFileIntegrity(filePath, fileName);
        }

        Main.Logger.LogInfo($"\nDownload {(success ? "Succeeded" : "Failed")}");
    }

    private static void VerifyFileIntegrity(string filePath, string fileName)
    {
        if (!File.Exists(filePath)) return;

        if (!ExpectedHashes.TryGetValue(fileName, out var expectedHash) || string.IsNullOrEmpty(expectedHash))
        {
            var actualHash = ComputeSHA256(filePath);
            Main.Logger.LogInfo($"SHA256 of {fileName}: {actualHash}");
            return;
        }

        var hash = ComputeSHA256(filePath);
        if (!string.Equals(hash, expectedHash, StringComparison.OrdinalIgnoreCase))
        {
            Main.Logger.LogError($"Hash mismatch for {fileName}! Expected: {expectedHash}, Got: {hash}");
            try { File.Delete(filePath); } catch { }
        }
    }

    private static string ComputeSHA256(string filePath)
    {
        using var sha = SHA256.Create();
        using var fs = File.OpenRead(filePath);
        var hash = sha.ComputeHash(fs);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
#endif
