using COG.Listener;
using COG.Utils;

namespace COG.Patch;

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.ShowRole))]
class SetUpRoleTextPatch
{
    [HarmonyPostfix]
    public static void Postfix(IntroCutscene __instance)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners())
        {
            listener.OnSetUpRoleText(__instance);
        }
    }
}

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
public static class BeginCrewmatePatch
{
    public static void Prefix(
        IntroCutscene __instance,
        ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners().ToList())
        {
            listener.OnSetUpTeamText(__instance, ref teamToDisplay);
        }
    }
}

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
public static class IntroCutsceneBeginImpostorPatch
{
    public static void Prefix(
        IntroCutscene __instance,
        ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners().ToList())
        {
            listener.OnSetUpTeamText(__instance, ref yourTeam);
        }
    }
}