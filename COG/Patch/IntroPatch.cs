using COG.Listener;
using COG.Listener.Event.Impl.ICutscene;
using Il2CppSystem.Collections.Generic;

namespace COG.Patch;

[HarmonyPatch(typeof(IntroCutscene._ShowRole_d__41), nameof(IntroCutscene._ShowRole_d__41.MoveNext))]
internal class SetUpRoleTextPatch
{
    public static void Postfix(IntroCutscene._ShowRole_d__41 __instance)
    {
        var @event = new IntroCutsceneShowRoleEvent(__instance);
        ListenerManager.GetManager().ExecuteHandlers(@event, EventHandlerType.Postfix);
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