using System.IO;
using System.Reflection;
using System.Text;

namespace COG.Utils;

public class ConfigManager
{
    private static bool inited = false;
    
    public static void Init()
    {
        if (inited) return;
        Directory.CreateDirectory(GetDataDirectoryName());
        
        Language.Init();
    }
    
    public static string GetResourcesString(string path)
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
        if (stream != null)
        {
            stream.Position = 0;
            using StreamReader reader = new(stream, Encoding.UTF8);
            return reader.ReadToEnd();
        }

        return null;
    }

    public static string GetDataDirectoryName()
    {
        return Main.PluginName + "_DATA";
    }
}