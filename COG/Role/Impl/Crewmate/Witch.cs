using COG.Listener;
using COG.Listener.Event.Impl.Meeting;
using COG.Rpc;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.Utils;
using COG.Utils.Coding;

namespace COG.Role.Impl.Crewmate;

[HarmonyPatch]
[Todo("多次死亡的检查")]
[NotUsed]
[WorkInProgress]
public class Witch : CustomRole
{
    private RpcHandler<byte> _antidoteHandler;
    private CustomOption _antidoteCooldown;
    private CustomButton _antidoteButton;

    private static bool _shouldDetectInteraction = false;
    private static bool _shouldDieWhenMeetingStarts = false;

    public Witch() : base()
    {
        _antidoteHandler = new(KnownRpc.WitchUsesAntidote,
            playerId =>
            {
                var player = PlayerUtils.GetPlayerById(playerId);
                if (!player)
                {
                    Main.Logger.LogWarning("Unknown player when witch reviving: " + playerId);
                    return;
                }
                player!.Revive();
                if (player.AmOwner)
                {
                    _shouldDetectInteraction = true;
                }
            },
            (writer, playerId) => writer.Write(playerId),
            reader => reader.ReadByte());
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnMeetingStarts(MeetingStartEvent @event)
    {
        if (_shouldDieWhenMeetingStarts)
            PlayerControl.LocalPlayer.RpcDie(CustomDeathReason.InteractionAfterRevival);
    }

    public override void ClearRoleGameData()
    {
        _shouldDetectInteraction = false;
        _shouldDieWhenMeetingStarts = false;
    }


    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.UseClosest))]
    [HarmonyPostfix]
    static void OnPlayerInteracts(PlayerControl __instance)
    {
        if (__instance.AmOwner && _shouldDetectInteraction)
            _shouldDieWhenMeetingStarts = true;
    }
}
