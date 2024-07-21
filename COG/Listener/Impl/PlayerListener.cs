using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using COG.Listener.Event.Impl.Player;
using COG.States;
using COG.UI.CustomOption;
using COG.Utils;
using UnityEngine;

namespace COG.Listener.Impl;

public class PlayerListener : IListener
{
    [EventHandler(EventHandlerType.Postfix)]
    public void OnJoinPlayer(PlayerControlAwakeEvent @event)
    {
        if (!GameStates.InGame) return; // Don't share option when the player prefab loaded (Scene MainMenu)
        var target = @event.Player;
        target.StartCoroutine(CoShareOptions().WrapToIl2Cpp());
        
        IEnumerator CoShareOptions()
        {
            Main.Logger.LogInfo($"Coroutine {nameof(CoShareOptions)} has started.");
            yield return new WaitForSeconds(0.1f);
            if (!target.IsSamePlayer(PlayerControl.LocalPlayer))
            {
                Main.Logger.LogInfo("Option info has sent to " + target.Data.PlayerName);
                CustomOption.ShareConfigs(target);
            }
        }
    }
}