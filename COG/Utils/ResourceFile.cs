using System.IO;
using System.Reflection;
using System.Text;

namespace COG.Utils;

public class ResourceFile
{
    public string Path { get; }
    
    public ResourceFile(string path)
    {
        Path = path;
    }
    
    public string GetResourcesText()
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(Path);
        stream.Position = 0;
        using StreamReader reader = new(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}