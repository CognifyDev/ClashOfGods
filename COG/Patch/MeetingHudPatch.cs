using System.Diagnostics.CodeAnalysis;
using COG.Listener;
using COG.Listener.Event.Impl.Meeting;
using COG.Utils;

namespace COG.Patch;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal class MeetingHudStartPatch
{
    public static void Postfix(MeetingHud __instance)
    {
        ListenerManager.GetManager()
            .ExecuteHandlers(new MeetingStartEvent(__instance), EventHandlerType.Postfix);
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.ServerStart))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal class MeetingHudServerStartPatch
{
    [HarmonyPostfix]
    public static void Postfix(MeetingHud __instance, [HarmonyArgument(0)] byte reporter)
    {
        ListenerManager.GetManager()
            .ExecuteHandlers(new MeetingServerStartEvent(__instance, reporter), EventHandlerType.Postfix);
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CastVote))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal class MeetingHudCastVotePatch
{
    private const byte SkipSuspectPlayerId = 253;

    public static bool Prefix(MeetingHud __instance, [HarmonyArgument(0)] byte srcPlayerId,
        [HarmonyArgument(1)] byte suspectPlayerId)
    {
        if (!AmongUsClient.Instance.AmHost) return false;

        var voterPlayer = PlayerUtils.GetPlayerById(srcPlayerId)!;
        var targetPlayer = PlayerUtils.GetPlayerById(suspectPlayerId);
        // 通常情况下voter作为发出请求方，voter的pc不会为null，target的pc因为退出可能会为null

        var isSkip = suspectPlayerId == SkipSuspectPlayerId;

        Main.Logger.LogDebug(
            $"{voterPlayer.Data.PlayerName} = Cast Vote => {(isSkip ? "Skipped" : targetPlayer == null ? "Unknown" : targetPlayer.Data.PlayerName)}");

        var result = ListenerManager.GetManager()
            .ExecuteHandlers(new MeetingCastVoteEvent(__instance, voterPlayer, targetPlayer, isSkip),
                EventHandlerType.Prefix);

        if (!result)
            __instance.RpcClearVote(voterPlayer.GetClientID());

        // 在listener中return false以取消玩家的投票事件，此后需要通过rpcClearVote清除玩家的投票操作，以允许玩家重新投票。
        return result;
    }

    public static void Postfix(MeetingHud __instance, [HarmonyArgument(0)] byte srcPlayerId,
        [HarmonyArgument(1)] byte suspectPlayerId)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        // CastVote行为通常只会由房主处理，因为client只会定向向host发送CastVote rpc.

        var voterPlayer = PlayerUtils.GetPlayerById(srcPlayerId)!;
        var targetPlayer = PlayerUtils.GetPlayerById(suspectPlayerId);

        var isSkip = suspectPlayerId == SkipSuspectPlayerId;

        ListenerManager.GetManager()
            .ExecuteHandlers(new MeetingCastVoteEvent(__instance, voterPlayer, targetPlayer, isSkip),
                EventHandlerType.Postfix);
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal class MeetingHudUpdatePatch
{
    private static int bufferTime = 10;

    // 缓冲时间，Update通常每1秒执行30次, bufferTime每次Update加1 !
    // 我们不需要像PlayerControl那样高频率执行patch，MeetingHud的绘图会导致卡顿
    public static void Postfix(MeetingHud __instance)
    {
        bufferTime--;
        if (bufferTime < 0 && __instance.discussionTimer > 0)
        {
            bufferTime = 10; // 这个值可能需要后续测试调整

            ListenerManager.GetManager()
                .ExecuteHandlers(new MeetingFixedUpdateEvent(__instance), EventHandlerType.Postfix);
        }
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal class MeetingHudVotingCompletePatch
{
    private static void Postfix(MeetingHud __instance, [HarmonyArgument(0)] MeetingHud.VoterState[] states,
        [HarmonyArgument(1)] NetworkedPlayerInfo exiled, [HarmonyArgument(2)] bool tie)
    {
        if (__instance.state == MeetingHud.VoteStates.Results) return;

        ListenerManager.GetManager()
            .ExecuteHandlers(new MeetingVotingCompleteEvent(__instance, states, exiled, tie), EventHandlerType.Postfix);
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal class MeetingHudCheckForEndVotingPatch
{
    private static bool Prefix(MeetingHud __instance)
    {
        return ListenerManager.GetManager()
            .ExecuteHandlers(new MeetingCheckForEndVotingEvent(__instance), EventHandlerType.Prefix);
    }
    
    private static void Postfix(MeetingHud __instance)
    {
        ListenerManager.GetManager()
            .ExecuteHandlers(new MeetingCheckForEndVotingEvent(__instance), EventHandlerType.Postfix);
    }
}