using COG.Constant;
using System;
using UnityEngine;
using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;

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
    public static T MarkDontUnload<T>(this T obj) where T : UnityEngine.Object
    {
        GameObject.DontDestroyOnLoad(obj);
        obj.hideFlags |= HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave;

        return obj;
    }
    /// <summary>
    /// ¥ª¥Ö¥¸¥§¥¯¥È¤Î<see cref="TextTranslatorTMP"/>¥³¥ó¥Ý©`¥Í¥ó¥È¤òÆÆ—‰¤·¤Þ¤¹
    /// </summary>
    public static void DestroyTranslator(this GameObject obj)
    {
        if (obj == null) return;
        obj.ForEachChild((Il2CppSystem.Action<GameObject>)DestroyTranslator);
        TextTranslatorTMP[] translator = obj.GetComponentsInChildren<TextTranslatorTMP>(true);
        translator?.Do(Object.Destroy);
    }
    public static T CreateObject<T>(string objName, Transform parent, Vector3 localPosition, int? layer = null)
        where T : Component
    {
        return CreateObject(objName, parent, localPosition, layer).AddComponent<T>();
    }
}