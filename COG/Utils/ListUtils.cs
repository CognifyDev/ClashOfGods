using System;
using System.Collections.Generic;

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
        var random = new Random();
        var resultList = new List<T>();
        foreach (var item in list)
        {
            resultList.Insert(random.Next(resultList.Count), item);
        }
        return resultList;
    }
}
