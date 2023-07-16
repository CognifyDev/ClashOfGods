using System.Collections.Generic;

namespace COG.Utils;

public static class ListUtils
{
    public static List<T> ToList<T>(this IEnumerable<T> enumerable)
    {
        return new List<T>(enumerable);
    }
}
