using System.Linq;
using COG.Utils;
using InnerNet;

namespace COG.Listener.Impl;

public class PlayerListener : IListener
{
    public void OnPlayerLeft(AmongUsClient client, ClientData data, DisconnectReasons reason)
    {
        foreach (var playerControl in from playerControl in PlayerUtils.Players
                 let typePlayer = client.PlayerPrefab
                 where playerControl.IsSamePlayer(typePlayer)
                 select playerControl) PlayerUtils.Players.Remove(playerControl);
    }
}