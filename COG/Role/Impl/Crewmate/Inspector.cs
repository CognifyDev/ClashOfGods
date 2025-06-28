using COG.Utils.Coding;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.Rpc;
using COG.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using COG.UI.CustomButton;

namespace COG.Role.Impl.Crewmate;

[NotTested]
[NotUsed]
[Todo("""
    1. Optimize repeated target searching button
    2. Add language strings
    3. Add cooldown option
    4. Add button sprite
    """)]
public class Inspector : CustomRole, IListener
{
    private bool _abilityUsedThisRound = false;
    private List<PlayerControl> _abilityUsedPlayers = new();
    private PlayerControl? _buttonTarget;
    private PlayerControl? _examinedTarget;

    public string YesString { get; }
    public string NoString { get; }
    public CustomButton ExamineButton { get; }

    public Inspector() : base(Color.gray, CampType.Crewmate)
    {
        OnRoleAbilityUsed += role => NotifyInspector();

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
                return PlayerControl.LocalPlayer.CheckClosestTargetInKillDistance(out _);
            },
            () => true,
            null!,
            2,
            KeyCode.X,
            "EXAMINE",
            () => 0,
            -1
       );
    }

    [EventHandler(EventHandlerType.Postfix)]
    [OnlyLocalPlayerWithThisRoleInvokable]
    public void OnRpcReceived(PlayerHandleRpcEvent @event)
    {
        if (@event.CallId is (byte)KnownRpc.ShareAbilityOrVentUseForInspector or (byte)RpcCalls.EnterVent)
            _abilityUsedPlayers.Add(@event.Player);
    }

    // The player of the role that vented/killed/used his ability performs this, so we gotta send RPC to notify
    public void NotifyInspector()
    {
        if (Players.Any() || IsLocalPlayerRole()) // If there are inspectors, send rpc; otherwise dont waste network traffic
            RpcUtils.StartAndSendRpc(KnownRpc.ShareAbilityOrVentUseForInspector);
    }

    public override void ClearRoleGameData()
    {
        _abilityUsedThisRound = false;
        _abilityUsedPlayers.Clear();
    }

    public override string HandleAdditionalPlayerName(PlayerControl player)
    {
        if (player.IsSamePlayer(_examinedTarget))
            return $"({(_abilityUsedPlayers.Contains(player) ? YesString.Color(Palette.ImpostorRed) : NoString.Color(Palette.ImpostorRed))})";
        return base.HandleAdditionalPlayerName(player);
    }

    public override IListener GetListener() => this;
}