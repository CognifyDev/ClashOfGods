using COG.Listener;
using COG.Utils;
using Il2CppSystem.Collections;
using Il2CppSystem.Collections.Generic;

namespace COG.Patch;

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.ShowRole))]
internal class SetUpRoleTextPatch
{
    [HarmonyPrefix]
    public static bool Prefix(IntroCutscene __instance, ref IEnumerator __result)
    {
        var toReturn = true;
        foreach (var listener in ListenerManager.GetManager().GetListeners())
            if (!listener.OnSetUpRoleText(__instance, ref __result))
                toReturn = false;

        return toReturn;
    }
}

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
public static class BeginCrewmatePatch
{
    public static void Prefix(
        IntroCutscene __instance,
        ref List<PlayerControl> teamToDisplay)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners().ToListCustom())
            listener.OnSetUpTeamText(__instance, ref teamToDisplay);
    }

    [HarmonyPostfix]
    public static void Postfix(IntroCutscene __instance)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners()) listener.AfterSetUpTeamText(__instance);
    }
}

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
public static class IntroCutsceneBeginImpostorPatch
{
    public static void Prefix(
        IntroCutscene __instance,
        ref List<PlayerControl> yourTeam)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners().ToListCustom())
            listener.OnSetUpTeamText(__instance, ref yourTeam);
    }

    [HarmonyPostfix]
    public static void Postfix(IntroCutscene __instance)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners()) listener.AfterSetUpTeamText(__instance);
    }
}