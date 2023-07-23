using System.Text.Json;
using Il2CppSystem.IO;

namespace COG.Utils.IO;

public class ObjectReader<T>
{
    public string Data { get; }

    public ObjectReader(string path)
    {
        Data = File.ReadAllText(path);
    }

    public T? Read()
    {
        return JsonSerializer.Deserialize<T>(Data);
    }

}