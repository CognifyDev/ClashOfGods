using System.Linq;
using System.Xml.Linq;

namespace COG.Utils;

public static class ProjectUtils
{
    /// <summary>
    ///     获取当前项目的版本
    /// </summary>
    /// <returns>项目版本</returns>
    public static string? GetProjectVersion()
    {
        try
        {
            return XDocument.Load(new ResourceFile("COG.COG.csproj").Stream!)
                .Descendants("Version").FirstOrDefault()?.Value;
        }
        catch
        {
            return null;
        }
    }
}