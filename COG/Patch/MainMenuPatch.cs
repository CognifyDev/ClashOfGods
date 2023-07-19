using System;
using System.Collections.Generic;
using COG.Config.Impl;
using COG.UI.ModOption;
using UnityEngine;

namespace COG.Patch;

[HarmonyPatch(typeof(MainMenuManager))]
public static class MainMenuPatch
{
    static GameObject? CustomBG = null;
    static List<PassiveButton> Buttons = new();

    [HarmonyPatch(nameof(MainMenuManager.Start))]
    [HarmonyPrefix]
    static void LoadButtons(MainMenuManager __instance)
    {
        Buttons.Clear();
        var template = __instance.creditsButton;
        
        if (!template) return;
        CreateButton(__instance, template, GameObject.Find("RightPanel")?.transform, new(0.25f, 0.15f), LanguageConfig.Instance.Github, () => { Application.OpenURL("https://github.com/CognifyDev/ClashOfGods/"); });
    }

    /// <summary>
    /// 在主界面创建一个按钮
    /// </summary>
    /// <param name="__instance">MainMenuManager 的实例</param>
    /// <param name="template">按钮模板</param>
    /// <param name="parent">父游戏物体</param>
    /// <param name="anchorPoint">与父游戏物体的相对位置</param>
    /// <param name="text">按钮文本</param>
    /// <param name="action">点击按钮的动作</param>
    /// <returns>返回这个按钮</returns>
    static void CreateButton(MainMenuManager __instance, PassiveButton template, Transform? parent, Vector2 anchorPoint, string text, Action action)
    {
        if (!parent) return;

        var button = UnityEngine.Object.Instantiate(template, parent);
        button.GetComponent<AspectPosition>().anchorPoint = anchorPoint;

        __instance.StartCoroutine(Effects.Lerp(0.5f, new Action<float>((p) => {
            button.GetComponentInChildren<TMPro.TMP_Text>().SetText(text);
        })));
        
        button.OnClick = new();
        button.OnClick.AddListener(action);

        Buttons.Add(button);
    }

    [HarmonyPatch(nameof(MainMenuManager.Start))]
    [HarmonyPostfix]
    static void LoadImage()
    {
        ModOption.Buttons.Clear();
        foreach (var modOption in ModOptionManager.GetManager().GetOptions())
        {
            modOption.Register();
        }
        
        CustomBG = new GameObject("CustomBG");
        CustomBG.transform.position = new Vector3(2f, 0f, 0f);
        var bgRenderer = CustomBG.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = Utils.ResourceUtils.LoadSprite("COG.Resources.InDLL.Images.COG-BG.png", 280f);
    }

    [HarmonyPatch(nameof(MainMenuManager.OpenAccountMenu))]
    [HarmonyPatch(nameof(MainMenuManager.OpenCredits))]
    [HarmonyPatch(nameof(MainMenuManager.OpenGameModeMenu))]
    [HarmonyPostfix]
    static void Hide()
    {
        if (CustomBG != null) CustomBG.SetActive(false);
        foreach (var btn in Buttons) btn.gameObject.SetActive(false);
    }
    [HarmonyPatch(nameof(MainMenuManager.ResetScreen))]
    [HarmonyPostfix]
    static void Show()
    {
        if (CustomBG != null) CustomBG.SetActive(true);
        foreach (var btn in Buttons)
        {
            if (btn == null || btn.gameObject == null) continue;
            btn.gameObject.SetActive(true);
        }
    }
}


