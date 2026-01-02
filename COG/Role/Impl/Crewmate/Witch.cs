using COG.Constant;
using COG.Listener;
using COG.Listener.Attribute;
using COG.Listener.Event.Impl.Meeting;
using COG.Listener.Event.Impl.Player;
using COG.Rpc;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.UI.Hud.CustomButton;
using COG.Utils;

namespace COG.Role.Impl.Crewmate;

[HarmonyPatch]
public class Witch : CustomRole, IListener
{
    private static bool _shouldDetectInteraction;
    private static bool _shouldDieWhenMeetingStarts;
    private readonly CustomButton _antidoteButton;
    private readonly CustomOption _antidoteCooldown;
    private readonly RpcHandler<byte> _antidoteHandler;

    private DeadBody? _current;
    private int _remainingUses = 1;

    public Witch() : base(ColorUtils.AsColor("#773ba4"), CampType.Crewmate)
    {
        _antidoteHandler = new RpcHandler<byte>(KnownRpc.WitchUsesAntidote,
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
                    OnRoleAbilityUsed += (role, _) => _shouldDieWhenMeetingStarts = true;
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
            ResourceUtils.LoadSprite(ResourceConstant.AntidoteButton)!,
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
        {
        } // TODO
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

    public override IListener GetListener()
    {
        return this;
    }


    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.UseClosest))]
    [HarmonyPostfix]
    private static void OnPlayerInteracts(PlayerControl __instance)
    {
        if (__instance.AmOwner && _shouldDetectInteraction)
            _shouldDieWhenMeetingStarts = true;
    }
}