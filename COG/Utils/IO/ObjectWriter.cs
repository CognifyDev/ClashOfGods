using System.IO;
using System.Text.Json;

namespace COG.Utils.IO;

public class ObjectWriter<T>
{
    public string Data { get; }

    public ObjectWriter(T obj)
    {
        Data = JsonSerializer.Serialize(obj);
    }

    public void WriteTo(string path)
    {
        File.WriteAllText(path, Data);
    }
}