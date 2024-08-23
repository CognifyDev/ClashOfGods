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
        var toReturn = "[";
        int i = 0;
        foreach (var item in collection)
        {
            toReturn += item + (i == collection.Count() - 1 ? "" : ", ");
            i++;
        }
        toReturn += "]";

        return toReturn;
    }

    public static string AsString<T>(this IEnumerable<T> collection, Func<T, string> selector)
    {
        var toReturn = "[";
        int i = 0;
        foreach (var item in collection)
        {
            toReturn += selector(item) + (i == collection.Count() - 1 ? "" : ", ");
            i++;
        }
        toReturn += "]";

        return toReturn;
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