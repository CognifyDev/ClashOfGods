using COG.Utils;
using System.Linq;

namespace COG.Patch;

[HarmonyPatch(typeof(HauntMenuMinigame))]
internal static class HauntMenuMinigamePatch
{
    [HarmonyPatch(nameof(HauntMenuMinigame.FixedUpdate))]
    [HarmonyPostfix]
    static void FixedUpdatePatch(HauntMenuMinigame __instance)
    {
        if (__instance.HauntTarget)
        {
            __instance.FilterText.text = string.Join(' ', __instance.HauntTarget.GetRoles().Select(r => r.GetColorName()));
        }
    }
}
