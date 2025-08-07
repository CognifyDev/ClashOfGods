using UnityEngine;

namespace COG.Utils;

public static class GameObjectUtils
{
    public static void TryDestroy(this Object? obj)
    {
        if (obj)
            Object.Destroy(obj);
    }

    public static void TryDestroyImmediate(this Object obj)
    {
        if (obj)
            Object.DestroyImmediate(obj);
    }

    public static void TryDestroyComponent<T>(this Component comp) where T : Component
    {
        comp.gameObject.TryDestroyComponent<T>();
    }

    public static void TryDestroyComponent<T>(this GameObject? obj) where T : Component
    {
        if (!obj) return;
        obj!.GetComponent<T>().TryDestroy();
    }
}