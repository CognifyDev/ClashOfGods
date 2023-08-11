using System.Reflection;

namespace COG.Utils;

public static class ProjectUtils
{
    /// <summary>
    /// 获取当前项目的版本
    /// </summary>
    /// <returns>项目版本</returns>
    public static string? GetProjectVersion() {
        var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
        return attributes.Length > 0 ? ((AssemblyFileVersionAttribute)attributes[0]).Version : null;
    }
}