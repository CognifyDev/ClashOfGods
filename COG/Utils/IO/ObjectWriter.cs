using System.IO;
using System.Text.Json;

namespace COG.Utils.IO;

public class ObjectWriter<T>
{
    public ObjectWriter(T obj)
    {
        Data = JsonSerializer.Serialize(obj);
    }

    public string Data { get; }

    public void WriteTo(string path)
    {
        File.WriteAllText(path, Data);
    }
}