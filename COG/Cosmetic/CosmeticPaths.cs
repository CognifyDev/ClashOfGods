using System;
using System.IO;
using BepInEx;
using COG.Config;
using UnityEngine;

namespace COG.Cosmetics;

public static class CosmeticPaths
{
    // 使用懒加载属性而非 static readonly 字段，
    // 避免在 BepInEx Paths 或 Application.persistentDataPath 尚未就绪时被提前求值。
    private static string? _basePath;
    public static string BasePath
    {
        get
        {
            if (_basePath != null) return _basePath;
            _basePath = Path.Combine(
                OperatingSystem.IsAndroid() ? Application.persistentDataPath : Paths.GameRootPath,
                ConfigBase.DataDirectoryName,
                "Cosmetics"
            );
            return _basePath;
        }
    }

    public static string ZipPath => BasePath;

    public static string CachePath => Path.Combine(BasePath, "Cache");

    /// <summary>
    /// 确保 Cosmetics 及 Cache 子目录存在，若不存在则自动创建。
    /// 在加载/解压任何装扮包之前必须先调用此方法。
    /// </summary>
    public static void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(BasePath);
        Directory.CreateDirectory(CachePath);
    }
}
