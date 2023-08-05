using System;
using System.Collections.Generic;
using COG.Config.Impl;
using COG.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace COG.Patch;

[HarmonyPatch(typeof(MainMenuManager))]
public static class MainMenuPatch
{
    private static GameObject? _customBg;
    private static readonly List<PassiveButton> Buttons = new();

    [HarmonyPatch(nameof(MainMenuManager.Start))]
    [HarmonyPrefix]
    private static void LoadButtons(MainMenuManager __instance)
    {
        Buttons.Clear();
        var template = __instance.creditsButton;

        if (!template) return;

        CreateButton(__instance, template, GameObject.Find("RightPanel")?.transform, new Vector2(0.2f, 0.38f),
            LanguageConfig.Instance.Github,
            () => { Application.OpenURL("https://github.com/CognifyDev/ClashOfGods/"); }, Color.blue);

        CreateButton(__instance, template, GameObject.Find("RightPanel")?.transform, new Vector2(0.7f, 0.38f),
            LanguageConfig.Instance.QQ,
            () =>
            {
                Application.OpenURL(
                    "https://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=R63D8foTESsZ9TmGFbkaSPix0q9WGwtq&authKey=1rfvioSJhdni%2BpFvBqS5rFXkZKXNDeeFO50ZKGPzwtlLKwmJqftlDcolx%2FkJ3jLC&noverify=0&group_code=607761127");
            }, Color.cyan);

        CreateButton(__instance, template, GameObject.Find("RightPanel")?.transform, new Vector2(0.45f, 0.38f),
            LanguageConfig.Instance.Discord, () => { Application.OpenURL("https://discord.gg/uWZGh4Chde"); },
            Color.gray);

        if (Main.BetaVersion)
            CreateButton(__instance, template, GameObject.Find("RightPanel")?.transform, new Vector2(0.45f, 0.38f / 2),
                LanguageConfig.Instance.BetaVersionRegisteredUserDisplay.CustomFormat(Main.RegisteredBetaUsers.Count),
                () => { }, Color.yellow);
    }

    /// <summary>
    ///     在主界面创建一个按钮
    /// </summary>
    /// <param name="__instance">MainMenuManager 的实例</param>
    /// <param name="template">按钮模板</param>
    /// <param name="parent">父游戏物体</param>
    /// <param name="anchorPoint">与父游戏物体的相对位置</param>
    /// <param name="text">按钮文本</param>
    /// <param name="action">点击按钮的动作</param>
    /// <returns>返回这个按钮</returns>
    private static void CreateButton(MainMenuManager __instance, PassiveButton template, Transform? parent,
        Vector2 anchorPoint, string text, Action action, Color color)
    {
        if (!parent) return;

        var button = Object.Instantiate(template, parent);
        button.GetComponent<AspectPosition>().anchorPoint = anchorPoint;
        var buttonSprite = button.transform.FindChild("Inactive").GetComponent<SpriteRenderer>();
        buttonSprite.color = color;
        __instance.StartCoroutine(Effects.Lerp(0.5f,
            new Action<float>(p => { button.GetComponentInChildren<TMP_Text>().SetText(text); })));

        button.OnClick = new Button.ButtonClickedEvent();
        button.OnClick.AddListener(action);

        Buttons.Add(button);
    }

    [HarmonyPatch(nameof(MainMenuManager.Start))]
    [HarmonyPostfix]
    private static void LoadImage()
    {
        _customBg = new GameObject("CustomBG");
        _customBg.transform.position = new Vector3(1.8f, 0.2f, 0f);
        var bgRenderer = _customBg.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = ResourceUtils.LoadSprite("COG.Resources.InDLL.Images.COG-BG.png", 295f);
    }

    [HarmonyPatch(nameof(MainMenuManager.OpenAccountMenu))]
    [HarmonyPatch(nameof(MainMenuManager.OpenCredits))]
    [HarmonyPatch(nameof(MainMenuManager.OpenGameModeMenu))]
    [HarmonyPostfix]
    private static void Hide()
    {
        if (_customBg != null) _customBg.SetActive(false);
        foreach (var btn in Buttons) btn.gameObject.SetActive(false);
    }

    [HarmonyPatch(nameof(MainMenuManager.ResetScreen))]
    [HarmonyPostfix]
    private static void Show()
    {
        if (_customBg != null) _customBg.SetActive(true);
        foreach (var btn in Buttons)
        {
            if (btn == null || btn.gameObject == null) continue;
            btn.gameObject.SetActive(true);
        }
    }
}