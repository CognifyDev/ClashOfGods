using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace COG.Utils;

public static class SerializationUtils
{
    public static byte[] SerializeToData(this object obj)
    {
        var formatter = new BinaryFormatter();
        using var stream = new MemoryStream();
#pragma warning disable
        formatter.Serialize(stream, obj);
#pragma warning restore
        return stream.ToArray();
    }

    public static T DeserializeToData<T>(this byte[] data)
    {
        var formatter = new BinaryFormatter();
        using var stream = new MemoryStream(data);
#pragma warning disable
        return (T)formatter.Deserialize(stream);
#pragma warning restore
    }
}