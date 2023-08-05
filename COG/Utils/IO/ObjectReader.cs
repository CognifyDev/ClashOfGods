using System.Text.Json;
using Il2CppSystem.IO;

namespace COG.Utils.IO;

public class ObjectReader<T>
{
    public ObjectReader(string path)
    {
        Data = File.ReadAllText(path);
    }

    public string Data { get; }

    public T? Read()
    {
        return JsonSerializer.Deserialize<T>(Data);
    }
}