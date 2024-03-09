namespace COG.Utils;

public static class ArrayUtils
{
    public static string AsString<T>(this T[] array)
    {
        var toReturn = "[";
        for (var i = 0; i < array.Length; i++) toReturn += array[i] + (i == array.Length - 1 ? "]" : ", ");

        return toReturn;
    }
}