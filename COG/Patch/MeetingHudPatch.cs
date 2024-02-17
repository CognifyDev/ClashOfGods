using COG.Listener;
using COG.Utils;
using System.Linq;


namespace COG.Patch;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
internal class MeetingHudStartPatch
{
    public static void Postfix(MeetingHud __instance)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners())
            listener.OnMeetingStart(__instance);
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CastVote))]
internal class MeetingHudCastVotePatch
{
    public static bool Prefix(MeetingHud __instance, [HarmonyArgument(0)] byte srcPlayerId, [HarmonyArgument(1)] byte suspectPlayerId)
    {
        if (!AmongUsClient.Instance.AmHost) return false;

        var returnAble = false;

        var voterPlayer = PlayerUtils.GetPlayerById(srcPlayerId);
        var targetPlayer = PlayerUtils.GetPlayerById(suspectPlayerId);
        // 通常情况下voter作为发出请求方，voter的pc不会为null，target的pc因为退出可能会为null

        if (voterPlayer == null || targetPlayer == null)
        {
            Main.Logger.LogWarning($"voter or target has a null pc, castvote check should be skipped. {srcPlayerId} => {suspectPlayerId}");
            returnAble = true;
        }

        Main.Logger.LogInfo($"{voterPlayer.Data.PlayerName} = Cast Vote => {targetPlayer.Data.PlayerName}");
        if (!returnAble)
            foreach (var unused in ListenerManager.GetManager().GetListeners()
                        .Where(listener => !listener.OnCastVote(__instance, voterPlayer, targetPlayer) && !returnAble)) returnAble = true;

        if (returnAble)
        {
            __instance.RpcClearVote(voterPlayer.GetClientID());
            __instance.CheckForEndVoting();
        }
        
        // 在listener中return false以取消玩家的投票事件，此后需要通过rpcClearVote清除玩家的投票操作，以允许玩家重新投票。
        return !returnAble;
    }

    public static void Postfix(MeetingHud __instance, [HarmonyArgument(0)] byte srcPlayerId, [HarmonyArgument(1)] byte suspectPlayerId)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        // CastVote行为通常只会由房主处理，因为client只会定向向host发送CastVote rpc.

        var voterPlayer = PlayerUtils.GetPlayerById(srcPlayerId);
        var targetPlayer = PlayerUtils.GetPlayerById(suspectPlayerId);

        Main.Logger.LogInfo($"{voterPlayer.Data.PlayerName} = Voted => {targetPlayer.Data.PlayerName}");

        if (voterPlayer == null || targetPlayer == null)
        {
            Main.Logger.LogWarning($"voter or target has a null pc. Voted check should not happen. {srcPlayerId} => {suspectPlayerId}");
            return;
        }

        foreach (var listener in ListenerManager.GetManager().GetListeners())
            listener.OnVoted(__instance, voterPlayer, targetPlayer);
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
internal class MeetingHudUpdatePatch
{
    private static int bufferTime = 10;
    // 缓冲时间，Update通常每1秒执行30次, bufferTime每次Update加1
    public static void Postfix(MeetingHud __instance)
    {
        bufferTime--;

        foreach (var listener in ListenerManager.GetManager().GetListeners())
            listener.OnMeetingHudUpdate(__instance);

        if (bufferTime < 0 && __instance.discussionTimer > 0)
        {
            bufferTime = 10;

            foreach (var listener in ListenerManager.GetManager().GetListeners())
                listener.OnMeetingHudLateUpdate(__instance);
        }
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateResults))]
internal class MeetingHudShowVoteResultsPatch
{
    public static void Postfix(MeetingHud __instance, [HarmonyArgument(0)] MeetingHud.VoterState[] states)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners())
            listener.OnVoteResultsShown(__instance);
    }

    // PopulateResults在VotingComplete里面被运行，但是VotingComplete可能被不在预期的情况下被调用
    // 因此我们patch PopulateResults，PopulateResults运行必然是向玩家展示投票结果之时
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
internal class MeetingHudCheckForEndVotingPatch
{
    public static bool Prefix(MeetingHud __instance)
    {
        return true;
        // 计票在此函数中运行，模组计票系统必须完全魔改此函数
        // 先空着，等以后在写
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CoStartCutscene))]
internal class MeetingHudCoStartCutscenePatch
{
    public static void Prefix(MeetingHud __instance)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners())
            listener.BeforeExileThePlayer(__instance, __instance.exiledPlayer, __instance.wasTie);
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.OnDestroy))]
internal class MeetingHudOnDestroyPatch
{
    public static void Postfix(MeetingHud __instance)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners())
            listener.OnMeetingFinished(__instance);
    }
}