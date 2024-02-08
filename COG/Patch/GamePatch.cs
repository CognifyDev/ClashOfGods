using COG.Listener;
using Il2CppSystem.Collections.Generic;
using System.Linq;

namespace COG.Patch;

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
internal class CoBeginPatch
{
    public static void Prefix()
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners()) listener.OnCoBegin();
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
internal class EndGamePatch
{
    public static void Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
    {
        var list = ListenerManager.GetManager().GetListeners().ToList();
        foreach (var listener in list) listener.OnGameEnd(__instance, ref endGameResult);
    }

    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
    {
        var list = ListenerManager.GetManager().GetListeners().ToList();
        foreach (var listener in list) listener.AfterGameEnd(__instance, ref endGameResult);
    }
}

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
internal class GameStartManagerStartPatch
{
    public static void Postfix(GameStartManager __instance)
    {
        HostStartPatch.Timer = 600f;

        foreach (var listener in ListenerManager.GetManager().GetListeners()) listener.OnGameStart(__instance);
    }
}

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.MakePublic))]
internal class MakePublicPatch
{
    public static bool Prefix(GameStartManager __instance)
    {
        var returnAble = false;
        foreach (var listener in ListenerManager.GetManager().GetListeners())
            if (!listener.OnMakePublic(__instance) && !returnAble)
                returnAble = true;

        if (returnAble) return false;

        return true;
    }
}

[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
internal class SelectRolesPatch
{
    public static void Prefix()
    {
        var listeners = ListenerManager.GetManager().GetListeners().ToList();
        foreach (var listener in listeners) listener.OnSelectRoles();
    }
}

[HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
internal class SetEverythingUpPatch
{
    public static void Postfix(EndGameManager __instance)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners())
            listener.OnGameEndSetEverythingUp(__instance);
    }
}

[HarmonyPatch(typeof(ControllerManager), nameof(ControllerManager.Update))]
internal class KeyboardPatch
{
    public static void Postfix()
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners()) listener.OnKeyboardPass();
    }
}

[HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
public static class PlayerVentPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Vent __instance,
        [HarmonyArgument(0)] GameData.PlayerInfo playerInfo,
        [HarmonyArgument(1)] ref bool canUse,
        [HarmonyArgument(2)] ref bool couldUse,
        ref float __result)
    {
        var returnAble = true;
        foreach (var listener in ListenerManager.GetManager().GetListeners())
            if (!listener.OnPlayerVent(__instance, playerInfo, ref canUse, ref couldUse, ref __result))
                returnAble = false;
        return returnAble;
    }
}

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
internal class GameEndChecker
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        var returnAble = true;
        foreach (var unused in
                 ListenerManager.GetManager().GetListeners().Where(listener => !listener.OnCheckGameEnd()))
            returnAble = false;

        return returnAble;
    }
}

[HarmonyPatch(typeof(GameManager), nameof(GameManager.CheckTaskCompletion))]
internal class CheckTaskCompletionPatch
{
    public static bool Prefix(ref bool __result)
    {
        var returnAble = true;
        foreach (var listener in ListenerManager.GetManager().GetListeners())
            if (!listener.OnCheckTaskCompletion(ref __result))
                returnAble = false;

        return returnAble;
    }
}

[HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowSabotageMap))]
internal class SabotageMapOpen
{
    private static bool Prefix(MapBehaviour __instance)
    {
        var returnAble = true;
        foreach (var unused in ListenerManager.GetManager().GetListeners()
                     .Where(listener => !listener.OnShowSabotageMap(__instance))) returnAble = false;

        return returnAble;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CoSetTasks))]
internal class TaskPatch
{
    public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] List<GameData.TaskInfo> tasks)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners()) listener.OnCoSetTasks(__instance, tasks);
    }
}

[HarmonyPatch(typeof(GameManager), nameof(GameManager.StartGame))]
internal class GameStartPatch
{
    public static void Postfix(GameManager __instance)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners()) listener.OnGameStartWithMovement(__instance);
    }
}

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
public class GameStartManagerUpdatePatch
{
    [HarmonyPrefix]
    public static void Prefix(GameStartManager __instance)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners())
            listener.OnGameStartManagerUpdate(__instance);
    }
}

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
public class GameStartManagerBeginGamePatch
{
    [HarmonyPostfix]
    public static void Postfix(GameStartManager __instance)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners())
            listener.OnGameStartCountdownEnd(__instance);
    }
}