using System;
using System.Collections;
using COG.Listener.Event.Impl.Player;
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
            return; // Don't share options when the player prefab loaded (Scene MainMenu)
        var target = @event.Player;
        Coroutines.Start(CoShareOptions());
        return;

        IEnumerator CoShareOptions()
        {
            yield return new WaitForSeconds(0.8f);
            if (!target.IsSamePlayer(PlayerControl.LocalPlayer))
            {
                const int maxAttempts = 3;
                for (var i = 0; i < maxAttempts; i++)
                {
                    var failed = false;
                    try
                    {
                        CustomOption.ShareConfigs(target);
                        break;
                    }
                    catch (SystemException ex)
                    {
                        Main.Logger.LogWarning(
                            $"Error while sending options: \n{ex}\nWill retry after 0.5s... ({i + 1}/{maxAttempts})");
                        failed = true;

                        if (i >= maxAttempts - 1)
                            Main.Logger.LogError("Failed to share options!");
                    }

                    if (failed && i < maxAttempts - 1)
                        yield return new WaitForSeconds(0.5f);
                }
            }
        }
    }
}