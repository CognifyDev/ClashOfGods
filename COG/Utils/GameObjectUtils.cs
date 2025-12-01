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

    public static void TryDestroyComponent<T>(this Component? comp) where T : Component
    {
        if (!comp) return;
        comp!.GetComponent<T>().TryDestroy();
    }

    public static void TryDestroyComponent<T>(this GameObject? obj) where T : Component
    {
        if (!obj) return;
        obj!.GetComponent<T>().TryDestroy();
    }

    public static void TryDestroyGameObject(this Component? comp)
    {
        if (!comp) return;
        comp!.gameObject.TryDestroy();
    }

    public static GameObject CreateObject(string objName, Transform parent, Vector3 localPosition, int? layer = null)
    {
        var obj = new GameObject(objName);
        obj.transform.SetParent(parent);
        obj.transform.localPosition = localPosition;
        obj.transform.localScale = new Vector3(1f, 1f, 1f);
        if (layer.HasValue)
            obj.layer = layer.Value;
        else if (parent != null)
            obj.layer = parent.gameObject.layer;
        return obj;
    }

    public static T CreateObject<T>(string objName, Transform parent, Vector3 localPosition, int? layer = null)
        where T : Component
    {
        return CreateObject(objName, parent, localPosition, layer).AddComponent<T>();
    }
}