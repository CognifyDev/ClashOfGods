using COG.Constant;
using COG.Listener;
using COG.Listener.Event.Impl.Meeting;
using COG.Listener.Event.Impl.Player;
using COG.Rpc;
using COG.Rpc.Role;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.UI.Hud.CustomButton;
using COG.Utils;

namespace COG.Role.Impl.Crewmate;

[HarmonyPatch]
public class Witch : COG.Role.Camp.CrewmateRole
{
    private static bool _shouldDetectInteraction;
    private static bool _shouldDieWhenMeetingStarts;
    private readonly CustomButton _antidoteButton;
    private readonly CustomOption _antidoteCooldown;
    private readonly RoleRpc<byte> _antidoteRpc;

    private DeadBody? _current;
    private int _remainingUses = 1;

    public Witch() : base(ColorUtils.AsColor("#773ba4"))
    {
        _antidoteRpc = CreateRoleRpc<byte>(KnownRpc.WitchUsesAntidote,
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
            });

        _antidoteCooldown = CreateOption(() => GetContextFromLanguage("antidote-cooldown"),
            new FloatOptionValueRule(10, 5, 60, 20, NumberSuffixes.Seconds));

        _antidoteButton = CustomButton.Builder("witch-antidote",
                ResourceConstant.AntidoteButton, ActionNameContext.GetString("antidote"))
            .OnClick(() =>
            {
                _antidoteRpc.PerformAndSend(_current!.ParentId);
                _remainingUses--;
            })
            .CouldUse(() => _remainingUses > 0 && (_current = PlayerUtils.GetClosestBody()))
            .Cooldown(_antidoteCooldown.GetFloat)
            .Build();

        AddButton(_antidoteButton);

        On<MeetingStartEvent>(OnMeetingStarts);
        On<PlayerTaskFinishEvent>(OnPlayerFinishesTask);
    }

    private void OnMeetingStarts(MeetingStartEvent @event)
    {
        if (_shouldDieWhenMeetingStarts) // Other players should always have this being false
        {
        } // TODO
    }

    private void OnPlayerFinishesTask(PlayerTaskFinishEvent @event)
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
    private static void OnPlayerInteracts(PlayerControl __instance)
    {
        if (__instance.AmOwner && _shouldDetectInteraction)
            _shouldDieWhenMeetingStarts = true;
    }
}