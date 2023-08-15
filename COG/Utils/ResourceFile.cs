using System.IO;
using System.Reflection;
using System.Text;

namespace COG.Utils;

public class ResourceFile
{
    public ResourceFile(string path)
    {
        Path = path;
        Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(Path);
    }

    public string Path { get; }
    public Stream? Stream { get; }

    public string GetResourcesText()
    {
        Stream!.Position = 0;
        using StreamReader reader = new(Stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}