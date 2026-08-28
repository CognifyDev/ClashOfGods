using System.Collections.Generic;
using System.Linq;
using COG.Config.Impl;
using COG.Constant;
using COG.Listener;
using COG.Rpc;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.UI.Hud.CustomButton;
using COG.Utils;
using COG.Utils.Coding;

namespace COG.Role.Impl.Crewmate;

public class Inspector : COG.Role.Camp.CrewmateRole
{
    private readonly List<PlayerControl> _abilityUsedPlayers = [];
    private bool _abilityUsedThisRound;
    private PlayerControl? _buttonTarget;
    private PlayerControl? _examinedTarget;

    public Inspector()
    {
        OnRoleAbilityUsed += (_, _) => NotifyInspector();

        AbilityCooldownOption = CreateOption(() => LanguageConfig.Instance.GetString("role.global.ability-cooldown"),
            new FloatOptionValueRule(10, 5, 60, 25, NumberSuffixes.Seconds));
        
        ExamineButton = CustomButton.Builder("inspector-examine",
                ResourceConstant.ExamineButton, LanguageConfig.Instance.GetString("action.examine"))
            .OnClick(() =>
            {
                _examinedTarget = _buttonTarget;
                _abilityUsedThisRound = true;
            })
            .OnMeetingEnds(() =>
            {
                _abilityUsedThisRound = false;
                ExamineButton?.ResetCooldown();
            })
            .CouldUse(() => !_abilityUsedThisRound && PlayerControl.LocalPlayer.CheckClosestTargetInKillDistance(out _buttonTarget))
            .Cooldown(AbilityCooldownOption.GetFloat)
            .Build();

        AddButton(ExamineButton);
    }

    public CustomOption AbilityCooldownOption { get; }
    public CustomButton ExamineButton { get; }

    public override void OnRpcReceived(PlayerControl sender, byte callId, MessageReader reader)
    {
        if (callId is (byte)KnownRpc.ShareAbilityOrVentUseForInspector or (byte)RpcCalls.EnterVent)
            _abilityUsedPlayers.Add(sender);
    }

    // The player of the role that vented/killed/used his ability performs this, so we gotta send RPC to notify
    public void NotifyInspector()
    {
        if (Players.Any() ||
            IsLocalPlayerRole()) // If there are inspectors, send rpc; otherwise dont waste network traffic
            RpcWriter.StartAndSend(KnownRpc.ShareAbilityOrVentUseForInspector);
    }

    public override void ClearRoleGameData()
    {
        _abilityUsedThisRound = false;
        _abilityUsedPlayers.Clear();
    }

    public override string HandleAdditionalPlayerName(PlayerControl player)
    {
        if (player.IsSamePlayer(_examinedTarget))
            return
                $"({(_abilityUsedPlayers.Contains(player) ? LanguageConfig.Instance.GetString("mod-global.yes").Color(Palette.ImpostorRed) : LanguageConfig.Instance.GetString("mod-global.no").Color(Palette.AcceptedGreen))})";
        return base.HandleAdditionalPlayerName(player);
    }
}