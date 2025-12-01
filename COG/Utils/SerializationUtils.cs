#pragma warning disable SYSLIB0011

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace COG.Utils;

public static class SerializationUtils
{
    public static byte[] SerializeToData(this object obj)
    {
        var formatter = new BinaryFormatter();
        using var stream = new MemoryStream();
        formatter.Serialize(stream, obj);
        return stream.ToArray();
    }

    public static T DeserializeToData<T>(this byte[] data)
    {
        var formatter = new BinaryFormatter();
        using var stream = new MemoryStream(data);
        return (T)formatter.Deserialize(stream);
    }
}