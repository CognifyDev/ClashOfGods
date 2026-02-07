using UnityEngine;

namespace COG.Utils;

public static class ReactorUtils
{
    public static byte[] ReadFully(this System.IO.Stream input)
    {
        using var memoryStream = new System.IO.MemoryStream();
        input.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
    public static T DontUnload<T>(this T obj) where T : UnityEngine.Object
    {
        obj.hideFlags |= HideFlags.DontUnloadUnusedAsset;
        return obj;
    }
}