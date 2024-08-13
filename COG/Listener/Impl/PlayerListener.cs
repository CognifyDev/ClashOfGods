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
            yield return new WaitForSeconds(0.8f);
            if (!target.IsSamePlayer(PlayerControl.LocalPlayer))
            {
                CustomOption.ShareConfigs(target);
            }
        }
    }
}