using System;
using System.IO;
using BepInEx;
using COG.Config;
using UnityEngine;

namespace COG.Cosmetics;

public static class CosmeticPaths
{
    public static string BasePath { get; } = Path.Combine(
        OperatingSystem.IsAndroid() ? Application.persistentDataPath : Paths.GameRootPath,
        ConfigBase.DataDirectoryName,
        "Cosmetics"
    );

    public static string ZipPath { get; } = BasePath;

    public static string CachePath { get; } = Path.Combine(BasePath, "Cache");

    public static void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(BasePath);
        Directory.CreateDirectory(CachePath);
    }
}
