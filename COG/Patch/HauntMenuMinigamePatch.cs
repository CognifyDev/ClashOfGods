using System.Linq;
using COG.Utils;

namespace COG.Patch;

[HarmonyPatch(typeof(HauntMenuMinigame))]
internal static class HauntMenuMinigamePatch
{
    [HarmonyPatch(nameof(HauntMenuMinigame.FixedUpdate))]
    [HarmonyPostfix]
    private static void FixedUpdatePatch(HauntMenuMinigame __instance)
    {
        if (__instance.HauntTarget)
            __instance.FilterText.text =
                string.Join(' ', __instance.HauntTarget.GetRoles().Select(r => r.GetColorName()));
    }
}