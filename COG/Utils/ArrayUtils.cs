using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace COG.Utils;

public static class ArrayUtils
{
    public static string AsString<T>(this T[] array)
    {
        var toReturn = "[";
        for (var i = 0; i < array.Length; i++) toReturn += array[i] + (i == array.Length - 1 ? "]" : ", ");

        return toReturn;
    }

    public static T[] ToSingleElementArray<T>(this T obj) => new[] { obj };

    public static Il2CppReferenceArray<T> ToIl2CppArray<T>(this T[] array) where T : Il2CppObjectBase => new(array);
}