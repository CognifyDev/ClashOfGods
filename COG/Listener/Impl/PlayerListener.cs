using Il2CppSystem.Collections;
using System.Collections.Generic;
using System.Threading;
using COG.Listener.Event.Impl.AuClient;
using COG.UI.CustomOption;
using COG.Utils;
using Il2CppSystem;
using UnityEngine;

namespace COG.Listener.Impl;

public class PlayerListener : IListener
{
    [EventHandler(EventHandlerType.Postfix)]
    public void OnJoinPlayer(AmongUsClientJoinEvent @event)
    {
        var instance = @event.AmongUsClient;

        List<IEnumerator> list = new()
        {
            Effects.ActionAfterDelay(0.5f, (Action)ShareCurrentOptions)
        };

        void ShareCurrentOptions()
        {
            Main.Logger.LogInfo($"Coroutine {nameof(ShareCurrentOptions)} has started.");
            var target = PlayerUtils.GetPlayerById(@event.ClientData.Character.PlayerId);
            if (!(target == null || target.IsSamePlayer(PlayerControl.LocalPlayer)))
            {
                Main.Logger.LogInfo("Sent options info to " + target.Data.PlayerName);
                CustomOption.ShareConfigs(target);
            }
        }

        instance.StartCoroutine(Effects.Sequence(list.ToArray()));
    }
}