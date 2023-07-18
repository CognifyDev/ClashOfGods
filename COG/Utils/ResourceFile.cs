using System.Diagnostics;
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
    
    public string GetResourcesTxt()
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(Path);
        Debug.Assert(stream != null, nameof(stream) + " != null");
        stream.Position = 0;
        using StreamReader reader = new(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}