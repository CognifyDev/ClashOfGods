using System;
using System.Collections.Generic;
using System.Linq;

namespace COG.Utils;

public static class ListUtils
{
    public static List<T> ToList<T>(this IEnumerable<T> enumerable)
    {
        return new List<T>(enumerable);
    }

    /// <summary>
    /// 打乱一个List
    /// </summary>
    /// <param name="list">欲打乱List</param>
    /// <typeparam name="T">元素类型</typeparam>
    /// <returns>打乱后的List</returns>
    public static List<T> Disarrange<T>(this List<T> list)
    {
        Random random = new Random();
        return list.OrderBy(_ => random.Next()).ToList();
    }

    public static T GetOneAndDelete<T>(this List<T> list)
    {
        var obj = list[0];
        list.RemoveAt(0);
        return obj;
    }
}
