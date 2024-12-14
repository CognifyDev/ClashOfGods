using System.IO;

namespace COG.Utils;

public static class StreamUtils
{
    public static byte[] ToBytes(this Stream stream)
    {
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        var bytes = memoryStream.ToArray();
        return bytes;
    }
}