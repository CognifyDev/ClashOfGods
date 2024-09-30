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
        var enumerable = collection as T[] ?? collection.ToArray();
        foreach (var item in enumerable)
        {
            toReturn += item + (i == enumerable.Length - 1 ? "" : ", ");
            i++;
        }
        toReturn += "]";

        return toReturn;
    }

    public static string AsString<T>(this IEnumerable<T> collection, Func<T, string> selector)
    {
        var toReturn = "[";
        int i = 0;
        var enumerable = collection as T[] ?? collection.ToArray();
        foreach (var item in enumerable)
        {
            toReturn += selector(item) + (i == enumerable.Length - 1 ? "" : ", ");
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