using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.Rpc;
using COG.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

public class Inspector : CustomRole, IListener
{
    private bool _abilityUsedThisRound = false;
    private List<PlayerControl> _abilityUsedPlayers = new();

    public Inspector() : base(Color.gray, CampType.Crewmate)
    {
        OnRoleAbilityUsed += role => NotifyInspector();
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnRpcReceived(PlayerHandleRpcEvent @event)
    {
        if (!IsLocalPlayerRole()) return;

        if (@event.CallId is (byte)KnownRpc.ShareAbilityOrVentUseForInspector or (byte)RpcCalls.EnterVent)
            _abilityUsedPlayers.Add(@event.Player);
    }

    // The player of the role that vented/killed/used his ability performs this, so we gotta send RPC to notify
    public void NotifyInspector()
    {
        if (Players.Any() || IsLocalPlayerRole()) // If there are inspectors, send rpc; otherwise dont waste network traffic
            RpcUtils.StartAndSendRpc(KnownRpc.ShareAbilityOrVentUseForInspector);
    }

    public override IListener GetListener() => this;
}