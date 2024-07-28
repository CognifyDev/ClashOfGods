using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppGenericCollections = Il2CppSystem.Collections.Generic;

namespace COG.Utils;

public static class ListUtils
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
        return list is not { Count: > 0 };
    }

    public static T GetOneAndDelete<T>(this List<T> list)
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

    public static Il2CppGenericCollections.List<T> ToIl2CppList<T>(this List<T> list)
    {
        var toReturn = new Il2CppGenericCollections.List<T>();
        list.ForEach(toReturn.Add);
        return toReturn;
    }
}