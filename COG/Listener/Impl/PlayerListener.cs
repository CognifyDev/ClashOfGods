using System.Collections;
using COG.Listener.Event.Impl.Player;
using COG.States;
using COG.UI.CustomOption;
using COG.Utils;
using Reactor.Utilities;
using UnityEngine;

namespace COG.Listener.Impl;

public class PlayerListener : IListener
{
    [EventHandler(EventHandlerType.Postfix)]
    public void OnJoinPlayer(PlayerControlAwakeEvent @event)
    {
        if (!GameStates.InLobby || !AmongUsClient.Instance.AmHost)
            return; // Don't share option when the player prefab loaded (Scene MainMenu)
        var target = @event.Player;
        Coroutines.Start(CoShareOptions());
        return;

        IEnumerator CoShareOptions()
        {
            Main.Logger.LogDebug($"Coroutine {nameof(CoShareOptions)} has started.");
            yield return new WaitForSeconds(0.1f);
            if (!target.IsSamePlayer(PlayerControl.LocalPlayer))
            {
                Main.Logger.LogDebug("Option info has sent to " + target.Data.PlayerName);
                CustomOption.ShareConfigs(target);
            }
        }
    }
}