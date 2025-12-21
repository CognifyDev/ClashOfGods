using System.Linq;
using COG.Utils;

namespace COG.Patch;

[HarmonyPatch]
internal static class ActionButtonPatch
{
    [HarmonyPatch(typeof(AbilityButton), nameof(AbilityButton.Update))]
    [HarmonyPrefix]
    private static bool AbilityButtonUpdatePatch()
    {
        return false;
        // Prevent showing comms down sprite on the buttons
    }
    [HarmonyPatch(typeof(VentButton), nameof(VentButton.DoClick))]
    [HarmonyPostfix]
    private static void VentButtonDoClick()
    {
        Main.Logger.LogInfo($"Local Player Impostor whether :{PlayerControl.LocalPlayer.Data.Role.IsImpostor}(Vent)");
        foreach (PlayerControl p in PlayerControl.AllPlayerControls)
        {
            Main.Logger.LogInfo($"Other Players Impostor whether :{p.Data.PlayerName}: {p.Data.Role.IsImpostor}(Vent)");
        }
        Main.Logger.LogInfo("VentButtonDoClick was done");
      
    }
    [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
    [HarmonyPostfix]
    private static void SabotageButtonDoClick()
    {
        Main.Logger.LogInfo($"Local Player Impostor whether :{PlayerControl.LocalPlayer.Data.Role.IsImpostor}(Sabotage)");
        foreach(PlayerControl p in PlayerControl.AllPlayerControls)
        {
            Main.Logger.LogInfo($"Other Players Impostor whether :{p.Data.PlayerName}: {p.Data.Role.IsImpostor}(Sabotage)");
        }
        Main.Logger.LogInfo("SabotageButton.DoClick was done");
    }
}