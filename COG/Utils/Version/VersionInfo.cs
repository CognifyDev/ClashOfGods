using System;
using System.Linq;

namespace COG.Utils.Version;

public class VersionInfo
{
    internal VersionInfo(string parent, bool beta = false, ulong? betaVersion = null)
    {
        Parent = parent;
        Beta = beta;
        BetaVersion = betaVersion;
    }

    public static VersionInfo Empty => new(string.Empty);

    public string Parent { get; }
    public bool Beta { get; }
    public ulong? BetaVersion { get; }

    public override string ToString()
    {
        return Beta ? $"{Parent}-beta.{BetaVersion}" : Parent;
    }

    /// <summary>
    ///     给定一个版本信息，与之相比，看看自身是否为最新的版本号
    /// </summary>
    /// <param name="versionInfo"></param>
    /// <returns>是否为最新版本号</returns>
    /// <exception cref="ArgumentException"></exception>
    public bool IsNewerThan(VersionInfo? versionInfo)
    {
        if (versionInfo == null) return false;
        if (Equals(versionInfo)) return false;

        var versionStrings = Parent.Split('.');
        var targetVersionStrings = versionInfo.Parent.Split('.');

        // 检查两个版本字符串的长度是否相等
        if (versionStrings.Length != targetVersionStrings.Length)
            throw new ArgumentException("Unequal number of segments for version number");

        if (versionInfo.Beta && BetaVersion > versionInfo.BetaVersion) return true;

        // 比较每个版本字符串，如果target版本字符串中的某个版本号在顺序上小于当前版本字符串中的对应版本号，则返回true
        for (var i = 0; i < versionStrings.Length; i++)
        {
            switch (string.Compare(targetVersionStrings[i], versionStrings[i], StringComparison.Ordinal))
            {
                case < 0:
                    return true;
                case <= 0:
                    continue;
            }

            if (!versionInfo.Beta)
                return false; // 如果target版本字符串中的某个版本号在顺序上大于当前版本字符串中的对应版本号，则返回false
        }

        // 如果所有检测都通过，但是目标版本是测试版，那么它一定要比目前新
        // 如果所有版本号都相等，那么当前版本并不比target版本新
        return !versionInfo.Beta;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not VersionInfo target) return false;

        return Beta.Equals(target.Beta) &&
               (BetaVersion == null ? target.BetaVersion == null : BetaVersion.Equals(target.BetaVersion)) &&
               Parent.Equals(target.Parent);
    }

    public override int GetHashCode()
    {
        return (Parent + Beta + (BetaVersion == null ? "" : BetaVersion)).GetHashCode();
    }

    /// <summary>
    ///     新建实例
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static VersionInfo Parse(string version)
    {
        if (!IsVersionString(version)) throw new ArgumentException("It must be a version string!");

        if (!IsBeta(version)) return new VersionInfo(version);

        try
        {
            var versionSpilt = version.Split("-beta.");
            var nowVersion = versionSpilt[0];
            var betaVersion = ulong.Parse(versionSpilt[1]);

            return new VersionInfo(nowVersion, true, betaVersion);
        }
        catch
        {
            throw new ArgumentException("It must be a version string!");
        }
    }

    private static bool IsVersionString(string version)
    {
        return
            version.Contains('.') &&
            version.Split('.').Length <= 4 &&
            version.Split('.').Length > 0 &&
            HasNumber(version);
    }

    private static bool IsBeta(string version)
    {
        return
            version.Contains("-beta.");
    }

    private static bool HasNumber(string text)
    {
        var numbers = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        return numbers.Any(number => text.Contains(number + string.Empty));
    }
}