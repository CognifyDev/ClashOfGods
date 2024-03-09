using COG.Listener;
using Il2CppSystem.Collections;
using Il2CppSystem.Collections.Generic;
using COG.Listener.Event.Impl.ICutscene;

namespace COG.Patch;

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.ShowRole))]
internal class SetUpRoleTextPatch
{
    [HarmonyPrefix]
    public static bool Prefix(IntroCutscene __instance, ref IEnumerator __result)
    {
        var @event = new IntroCutsceneShowRoleEvent(__instance, __result);
        var result = ListenerManager.GetManager().ExecuteHandlers(@event, EventHandlerType.Prefix);
        __result = @event.GetResult();
        return result;
    }
}

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
public static class BeginCrewmatePatch
{
    [HarmonyPrefix]
    public static bool Prefix(
        IntroCutscene __instance,
        ref List<PlayerControl> teamToDisplay)
    {
        var @event = new IntroCutsceneBeginCrewmateEvent(__instance, teamToDisplay);
        var result = ListenerManager.GetManager().ExecuteHandlers(@event, EventHandlerType.Prefix);
        teamToDisplay = @event.GetTeamToDisplay();
        return result;
    }

    [HarmonyPostfix]
    public static void Postfix(IntroCutscene __instance, ref List<PlayerControl> teamToDisplay)
    {
        var @event = new IntroCutsceneBeginCrewmateEvent(__instance, teamToDisplay);
        ListenerManager.GetManager().ExecuteHandlers(@event, EventHandlerType.Postfix);
        teamToDisplay = @event.GetTeamToDisplay();
    }
}

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
public static class IntroCutsceneBeginImpostorPatch
{
    [HarmonyPrefix]
    public static bool Prefix(
        IntroCutscene __instance,
        ref List<PlayerControl> yourTeam)
    {
        var @event = new IntroCutsceneBeginImpostorEvent(__instance, yourTeam);
        var result = ListenerManager.GetManager().ExecuteHandlers(@event, EventHandlerType.Prefix);
        yourTeam = @event.GetYourTeam();
        return result;
    }

    [HarmonyPostfix]
    public static void Postfix(IntroCutscene __instance, ref List<PlayerControl> yourTeam)
    {
        var @event = new IntroCutsceneBeginImpostorEvent(__instance, yourTeam);
        var result = ListenerManager.GetManager().ExecuteHandlers(@event, EventHandlerType.Postfix);
        yourTeam = @event.GetYourTeam();
    }
}