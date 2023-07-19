using COG.Listener;
using HarmonyLib;

namespace COG.Patch;

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.ShowRole))]
class SetUpRoleTextPatch
{
    public static void Postfix(IntroCutscene __instance)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners())
        {
            listener.OnSetUpRoleText(__instance);
        }
    }
}

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.ShowTeam))]
class SetUpTeamTextPatch
{
    public static void Postfix(IntroCutscene __instance)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners())
        {
            listener.OnSetUpTeamText(__instance);
        }
    }
}