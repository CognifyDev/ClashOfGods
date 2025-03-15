using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;
using System.Collections.Generic;
using System.Linq;

namespace COG.Utils;

public static class ArrayUtils
{
    public static string AsString<T>(this IEnumerable<T> collection)
    {
        return $"[{string.Join(", ", collection)}]";
    }

    public static string AsString<T>(this IEnumerable<T> collection, Func<T, string> selector)
    {
        return $"[{string.Join(", ", collection.Select(selector))}]";
    }

    public static T[] ToSingleElementArray<T>(this T obj)
    {
        return new[] { obj };
    }

    public static Il2CppReferenceArray<T> ToIl2CppArray<T>(this T[] array) where T : Il2CppObjectBase
    {
        return new Il2CppReferenceArray<T>(array);
    }
}