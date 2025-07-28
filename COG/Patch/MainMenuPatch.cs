using COG.Config.Impl;
using COG.Utils;
using COG.Utils.Coding.Debugging;
using COG.Utils.Version;
using COG.Utils.WinAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace COG.Patch;

[HarmonyPatch(typeof(MainMenuManager))]
public static class MainMenuPatch
{
    public static GameObject? CustomBanner;
    public static readonly List<PassiveButton> Buttons = new();
    public static bool PopupCreated;

    [HarmonyPatch(nameof(MainMenuManager.Start))]
    [HarmonyPrefix]
    private static void LoadButtons(MainMenuManager __instance)
    {
        Buttons.Clear();
        var template = __instance.creditsButton;

        if (!template) return;

        CreateButton(__instance, template, GameObject.Find("RightPanel")?.transform, new Vector2(0.2f, 0.38f),
            LanguageConfig.Instance.GitHub,
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

        //if (!VersionInfo.Empty.Equals(ModUpdater.LatestVersion) || 
        //    (ModUpdater.LatestVersion != null && !ModUpdater.LatestVersion.IsNewerThan(Main.VersionInfo)))
        //    CreateButton(__instance, template, GameObject.Find("RightPanel")?.transform, new Vector2(0.45f, 0.38f / 2),
        //        LanguageConfig.Instance.UpdateButtonString, () =>
        //        {
        //            var result = SystemUtils.OpenMessageBox(
        //                LanguageConfig.Instance.FetchedString.CustomFormat(ModUpdater.LatestVersion!.ToString(),
        //                    ModUpdater.LatestDescription), "Update COG",
        //                MessageBoxDialogue.OpenTypes.MB_ICONQUESTION | MessageBoxDialogue.OpenTypes.MB_YESNO);

        //            if (result is MessageBoxDialogue.ClickedButton.IDNO) return;

        //            ModUpdater.DoUpdate();
        //            Application.Quit();
        //        }, Color.yellow);

        if (!InGameCustomRoleViewer.Instance)
            new GameObject().AddComponent<InGameCustomRoleViewer>();

        __instance.createGameScreen.modeButtons[0].SelectButton(true);
        __instance.createGameScreen.modeButtons[1].SelectButton(false);
        __instance.createGameScreen.modeButtons[1].gameObject.SetActive(false); // Hide HnS button & select classic button
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
            new Action<float>(_ => { button.GetComponentInChildren<TMP_Text>().SetText(text); })));

        button.OnClick = new Button.ButtonClickedEvent();
        button.OnClick.AddListener(action);

        Buttons.Add(button);
    }

    [HarmonyPatch(nameof(MainMenuManager.Start))]
    [HarmonyPostfix]
    private static void LoadImage()
    {
        CustomBanner = new GameObject("CustomBG");
        CustomBanner.transform.position = new Vector3(1.8f, 0.2f, 0f);
        var bgRenderer = CustomBanner.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = ResourceUtils.LoadSprite("COG.Resources.Images.COG-BG.png", 295f);
    }

    [HarmonyPatch(nameof(MainMenuManager.Start))]
    [HarmonyPostfix]
    private static void InitPopup()
    {
        if (PopupCreated) return;
        var popup = Object.Instantiate(DiscordManager.Instance.discordPopup);
        popup.destroyOnClose = true;
        var bg = popup.transform.Find("Background").GetComponent<SpriteRenderer>();
        var bgSize = bg.size;
        bgSize.x *= 2.5f;
        bg.size = bgSize;
        popup.name = "COGPopup";
        popup.TextAreaTMP.fontSizeMin = popup.TextAreaTMP.fontSizeMax = popup.TextAreaTMP.fontSize;
        Object.DontDestroyOnLoad(popup);
        GameUtils.PopupPrefab = popup;
        PopupCreated = true;
    }

    [HarmonyPatch(nameof(MainMenuManager.OpenAccountMenu))]
    [HarmonyPatch(nameof(MainMenuManager.OpenCredits))]
    [HarmonyPatch(nameof(MainMenuManager.OpenGameModeMenu))]
    [HarmonyPatch(nameof(MainMenuManager.OpenOnlineMenu))]
    [HarmonyPostfix]
    private static void HideModBannerPatch()
    {
        if (CustomBanner != null) CustomBanner.SetActive(false);
        foreach (var btn in Buttons) btn.gameObject.SetActive(false);
    }

    [HarmonyPatch(nameof(MainMenuManager.ResetScreen))]
    [HarmonyPostfix]
    private static void ShowModBackground()
    {
        if (CustomBanner != null) CustomBanner.SetActive(true);
        foreach (var btn in Buttons.Where(btn => btn != null && btn.gameObject != null)) btn.gameObject.SetActive(true);
    }
}