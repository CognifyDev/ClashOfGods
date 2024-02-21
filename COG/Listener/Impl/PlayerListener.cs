using System.Threading;
using COG.Listener.Event.Impl.AuClient;
using COG.UI.CustomOption;
using COG.Utils;

namespace COG.Listener.Impl;

public class PlayerListener : IListener
{
    [EventHandler(EventHandlerType.Postfix)]
    public void OnCreatePlayer(AmongUsClientCreatePlayerEvent @event)
    {
        var thread = new Thread(() =>
        {
            Thread.Sleep(500);
            var target = PlayerUtils.GetPlayerById(@event.ClientData.Character.PlayerId);
            if (target == null || target.IsSamePlayer(PlayerControl.LocalPlayer)) return;
            Main.Logger.LogInfo("Sent options info to " + target.Data.PlayerName);
            CustomOption.ShareConfigs(target);
        });
        thread.Start();
    }
}