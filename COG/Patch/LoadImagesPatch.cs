using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace COG;
[HarmonyPatch(typeof(MainMenuManager))]
public static class TitleLogo
{
    static GameObject? CustomBG = null;

    [HarmonyPatch(nameof(MainMenuManager.Start))]
    [HarmonyPostfix]
    static void LoadImage()
    {
        CustomBG = new GameObject("CustomBG");
        CustomBG.transform.position = new Vector3(2, 0f, 0);
        var bgRenderer = CustomBG.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = Utils.ResourceUtils.LoadSprite("COG.Resources.InDLL.Images.COG-BG.png", 280f);
    }
    [HarmonyPatch(nameof(MainMenuManager.OpenAccountMenu))]
    [HarmonyPatch(nameof(MainMenuManager.OpenCredits))]
    [HarmonyPatch(nameof(MainMenuManager.OpenGameModeMenu))]
    [HarmonyPostfix]
    static void HideImage()
    {
        CustomBG?.SetActive(false);
    }
    [HarmonyPatch(nameof(MainMenuManager.ResetScreen))]
    [HarmonyPostfix]
    static void ShowImage()
    {
        CustomBG?.SetActive(true);
    }
}


