using System.Linq;
using COG.Utils;
using UnityEngine;

namespace COG.Patch;

[HarmonyPatch(typeof(GameStartManager))]
internal static class GameStartManagerPatch
{
    private static bool Opened;
    private static GameObject? Menu;

    [HarmonyPatch(nameof(GameStartManager.ToggleViewPane))]
    [HarmonyPrefix]
    public static bool ViewSettingPatch(GameStartManager __instance)
    {
        if (Opened)
        {
            if (Menu != null) Menu.gameObject.Destroy();
            Menu = null;
            Opened = false;
        }
        else
        {
            PlayerControl.LocalPlayer.NetTransform.Halt();
            Menu = Object.Instantiate(__instance.PlayerOptionsMenu, Camera.main.transform);
            Menu.transform.localPosition = __instance.GameOptionsPosition;
            Menu.transform.FindChild("Background").gameObject.SetActive(false);
            __instance.RulesViewPanel.SetActive(false);
            __instance.SelectViewButton(false);
            __instance.LobbyInfoPane.DeactivatePane();
            Opened = true;
        }
        return false;
    }

    [HarmonyPatch(nameof(GameStartManager.Update))]
    [HarmonyPostfix]
    public static void OnManagerUpdate(GameStartManager __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        __instance.HostViewButton.gameObject.SetActive(false);
        var button = __instance.EditButton;
        button.transform.localPosition = new(-1, -0.1f, 0);
        button.inactiveSprites.transform.localScale = button.activeSprites.transform.localScale = new(2, 1, 1);
        var collider = button.Colliders.First().Cast<BoxCollider2D>();
        var size = button.inactiveSprites.GetComponent<SpriteRenderer>().size;
        size.x *= 2;
        collider.size = size;
    }
}