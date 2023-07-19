using UnityEngine;

namespace COG.Patch;

[HarmonyPatch(typeof(MainMenuManager))]
public static class TitleLogo
{
    private static GameObject? _customBg;

    [HarmonyPatch(nameof(MainMenuManager.Start))]
    [HarmonyPostfix]
    static void LoadImage()
    {
        _customBg = new GameObject("CustomBG")
        {
            transform =
            {
                position = new Vector3(2, 0f, 0)
            }
        };
        var bgRenderer = _customBg.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = Utils.ResourceUtils.LoadSprite("COG.Resources.InDLL.Images.COG-BG.png", 280f);
    }
    [HarmonyPatch(nameof(MainMenuManager.OpenAccountMenu))]
    [HarmonyPatch(nameof(MainMenuManager.OpenCredits))]
    [HarmonyPatch(nameof(MainMenuManager.OpenGameModeMenu))]
    [HarmonyPostfix]
    static void HideImage()
    {
        if (_customBg != null)
            _customBg.SetActive(false);
    }
    [HarmonyPatch(nameof(MainMenuManager.ResetScreen))]
    [HarmonyPostfix]
    static void ShowImage()
    {
        if (_customBg != null) 
            _customBg.SetActive(true);
    }
}