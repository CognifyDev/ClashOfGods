using COG.Listener;
using COG.Listener.Event.Impl.Meeting;
using COG.Listener.Event.Impl.Player;
using COG.Rpc;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;

namespace COG.Role.Impl.Crewmate;

[HarmonyPatch]
public class Witch : CustomRole
{
    private RpcHandler<byte> _antidoteHandler;
    private CustomOption _antidoteCooldown;
    private CustomButton _antidoteButton;

    private DeadBody? _current;
    private int _remainingUses = 1;

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

        _antidoteCooldown = CreateOption(() => GetContextFromLanguage("antidote-cooldown"),
            new FloatOptionValueRule(10, 5, 60, 20, NumberSuffixes.Seconds));

        _antidoteButton = CustomButton.Of("witch-antidote",
            () =>
            {
                _antidoteHandler.PerformAndSend(_current!.ParentId);
                _remainingUses--;
            },
            () => { },
            () => _remainingUses > 0 && (_current = PlayerUtils.GetClosestBody()),
            () => true,
            null!,
            2,
            ActionNameContext.GetString("antidote"),
            () => _antidoteCooldown.GetFloat(),
            0);

        AddButton(_antidoteButton);
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnMeetingStarts(MeetingStartEvent @event)
    {
        if (_shouldDieWhenMeetingStarts) // Other players should always have this being false
            PlayerControl.LocalPlayer.RpcDie(CustomDeathReason.InteractionAfterRevival);
    }

    [EventHandler(EventHandlerType.Postfix)]
    [OnlyLocalPlayerWithThisRoleInvokable]
    public void OnPlayerFinishesTask(PlayerTaskFinishEvent @event)
    {
        if (@event.Player.AllTasksCompleted())
            _remainingUses++;
    }

    public override void ClearRoleGameData()
    {
        _remainingUses = 1;
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
