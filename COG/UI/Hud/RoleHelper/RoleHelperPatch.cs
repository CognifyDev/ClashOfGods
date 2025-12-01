using TMPro;
using UnityEngine;

namespace COG.UI.Hud.RoleHelper;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class RoleHelperPatch
{
    public static GameObject Panel;
    public static TextMeshPro Title;
    public static TextMeshPro SubTitle;
    public static TextMeshPro Text;

    public static void IntiAll(HudManager hud)
    {
    }
}