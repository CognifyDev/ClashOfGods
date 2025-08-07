using COG.Utils.Coding;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.Rpc;
using COG.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using COG.Config.Impl;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Constant;
using COG.UI.Hud.CustomButton;

namespace COG.Role.Impl.Crewmate;

public class Inspector : CustomRole, IListener
{
    private bool _abilityUsedThisRound = false;
    private List<PlayerControl> _abilityUsedPlayers = new();
    private PlayerControl? _buttonTarget;
    private PlayerControl? _examinedTarget;

    public CustomOption AbilityCooldownOption { get; }
    public CustomButton ExamineButton { get; }

    public Inspector() : base(ColorUtils.FromColor32(46, 84, 160), CampType.Crewmate)
    {
        OnRoleAbilityUsed += (role, _) => NotifyInspector();

        AbilityCooldownOption = CreateOption(() => LanguageConfig.Instance.AbilityCooldown,
            new FloatOptionValueRule(10, 5, 60, 25, NumberSuffixes.Seconds));
        ExamineButton = CustomButton.Of("inspector-examine",
            () =>
            {
                _examinedTarget = _buttonTarget;
                _abilityUsedThisRound = true;
            },
            () =>
            {
                _abilityUsedThisRound = false;
                ExamineButton?.ResetCooldown();
            },
            () =>
            {
                if (_abilityUsedThisRound) 
                    return false;
                return PlayerControl.LocalPlayer.CheckClosestTargetInKillDistance(out _buttonTarget);
            },
            () => true,
            ResourceUtils.LoadSprite(ResourceConstant.ExamineButton)!,
            2,
            LanguageConfig.Instance.ExamineAction,
            AbilityCooldownOption.GetFloat,
            -1
        );

        AddButton(ExamineButton);
    }

    [OnlyLocalPlayerWithThisRoleInvokable]
    public override void OnRpcReceived(PlayerControl sender, byte callId, MessageReader reader)
    {
        if (callId is (byte)KnownRpc.ShareAbilityOrVentUseForInspector or (byte)RpcCalls.EnterVent)
            _abilityUsedPlayers.Add(sender);
    }

    // The player of the role that vented/killed/used his ability performs this, so we gotta send RPC to notify
    public void NotifyInspector()
    {
        if (Players.Any() || IsLocalPlayerRole()) // If there are inspectors, send rpc; otherwise dont waste network traffic
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
            return $"({(_abilityUsedPlayers.Contains(player) ? LanguageConfig.Instance.Yes.Color(Palette.ImpostorRed) : LanguageConfig.Instance.No.Color(Palette.AcceptedGreen))})";
        return base.HandleAdditionalPlayerName(player);
    }

    public override IListener GetListener() => this;
}