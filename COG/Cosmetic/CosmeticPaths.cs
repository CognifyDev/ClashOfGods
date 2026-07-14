using System.IO;
using COG.Config;

namespace COG.Cosmetics;

public static class CosmeticPaths
{
    public static string BasePath { get; } = Path.Combine(ConfigBase.BasePath, "Cosmetics");

    public static string ZipPath { get; } = BasePath;

    public static string CachePath { get; } = Path.Combine(BasePath, "Cache");

    public static void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(BasePath);
        Directory.CreateDirectory(CachePath);
    }
}
