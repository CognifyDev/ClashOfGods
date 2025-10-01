using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppGenericCollections = Il2CppSystem.Collections.Generic;

namespace COG.Utils;

public static class CollectionUtils
{
    /// <summary>
    ///     打乱一个List
    /// </summary>
    /// <param name="list">欲打乱List</param>
    /// <typeparam name="T">元素类型</typeparam>
    /// <returns>打乱后的List</returns>
    public static List<T> Disarrange<T>(this IEnumerable<T> list)
    {
        return list.OrderBy(_ => new Random().Next()).ToList();
    }

    public static bool IsEmpty<T>(this List<T> list)
    {
        return !list.Any();
    }

    public static T Pop<T>(this List<T> list)
    {
        var obj = list[0];
        list.RemoveAt(0);
        return obj;
    }

    public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
    {
        collection.ToList().ForEach(action);
    }

    // Using methods that contain arguments with keyword out such as TryGetValue(TKey key, out TValue value) may cause some unexcepted situations because of Il2CppInterop
    public static bool TryGetValueSafeIl2Cpp<TKey, TValue>(this Il2CppGenericCollections.Dictionary<TKey, TValue> dic,
        TKey key, out TValue safeValue) where TKey : notnull
    {
        if (dic.ContainsKey(key))
        {
            safeValue = dic[key];
            return true;
        }

        safeValue = default!;
        return false;
    }

    public static Il2CppGenericCollections.List<T> ToIl2CppList<T>(this IEnumerable<T> list)
    {
        var toReturn = new Il2CppGenericCollections.List<T>();
        list.ForEach(toReturn.Add);
        return toReturn;
    }

    public static ReadOnlyCollection<T> AsReadonly<T>(this List<T> list)
    {
        return new ReadOnlyCollection<T>(list);
    }

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