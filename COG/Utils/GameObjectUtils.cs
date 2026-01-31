using COG.Constant;
using System;
using UnityEngine;
using System.Collections;
using COG.UI.MetaContext;
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
    static public SpriteRenderer CreateSharpBackground(Vector2 size, Color color, Transform transform)
    {
        var renderer = CreateObject<SpriteRenderer>("Background", transform, new Vector3(0, 0, 0.25f));
        return CreateSharpBackground(renderer, color, size);
    }
    static public SpriteRenderer CreateSharpBackground(SpriteRenderer renderer, Color color, Vector2 size)
    {
        renderer.sprite = ResourceConstant.SharpWindowBackgroundSprite.GetSprite();
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.tileMode = SpriteTileMode.Continuous;
        renderer.color = color;
        renderer.size = size;
        return renderer;
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
    /// <summary>
    /// ¥ª¥Ö¥¸¥§¥¯¥È¤Î<see cref="TextTranslatorTMP"/>¥³¥ó¥Ý©`¥Í¥ó¥È¤òÆÆ—‰¤·¤Þ¤¹
    /// </summary>
    public static void DestroyTranslatorL(this MonoBehaviour obj) => obj?.gameObject?.DestroyTranslator();
    public static T CreateObject<T>(string objName, Transform parent, Vector3 localPosition, int? layer = null)
        where T : Component
    {
        return CreateObject(objName, parent, localPosition, layer).AddComponent<T>();
    }
    public static void SetModText(this TextTranslatorTMP text, string translationKey)
    {
        text.TargetText = (StringNames)short.MaxValue;
        text.defaultStr = translationKey;
    }
    public static PassiveButton SetUpButton(this GameObject gameObject, bool withSound = false, SpriteRenderer buttonRenderer = null, Color? defaultColor = null, Color? selectedColor = null)
    {
        var button = gameObject.AddComponent<PassiveButton>();
        button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        button.OnMouseOut = new UnityEngine.Events.UnityEvent();
        button.OnMouseOver = new UnityEngine.Events.UnityEvent();

        if (withSound)
        {
            button.OnClick.AddListener((Action)(() => VanillaAsset.PlaySelectSE()));
            button.OnMouseOver.AddListener((Action)(() => VanillaAsset.PlayHoverSE()));
        }
        if (buttonRenderer != null)
        {
            button.OnMouseOut.AddListener((Action)(() => buttonRenderer!.color = defaultColor ?? Color.white));
            button.OnMouseOver.AddListener((Action)(() => buttonRenderer!.color = selectedColor ?? Color.green));
        }

        if (buttonRenderer != null) buttonRenderer.color = defaultColor ?? Color.white;

        return button;
    }
    public static bool IsPiled(this PassiveUiElement uiElem)
    {
        var currentOver = PassiveButtonManager.Instance.currentOver;
        if (!currentOver || !uiElem) return false;
        return currentOver.GetInstanceID() == uiElem.GetInstanceID();
    }
    public static void DoTransitionFade(this TransitionFade transitionFade, GameObject transitionFrom, GameObject transitionTo, System.Action onTransition, System.Action callback)
    {
        if (transitionTo) transitionTo!.SetActive(false);

        IEnumerator Coroutine()
        {
            yield return Effects.ColorFade(transitionFade.overlay, Color.clear, Color.black, 0.1f);
            if (transitionFrom && transitionFrom!.gameObject) transitionFrom.gameObject.SetActive(false);
            if (transitionTo && transitionTo!.gameObject) if (transitionTo != null) transitionTo.gameObject.SetActive(true);
            onTransition.Invoke();
            yield return null;
            yield return Effects.ColorFade(transitionFade.overlay, Color.black, Color.clear, 0.1f);
            callback.Invoke();
            yield break;
        }

        transitionFade.StartCoroutine(Coroutine().WrapToIl2Cpp());
    }
}