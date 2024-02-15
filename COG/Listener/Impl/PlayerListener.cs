using System.Threading;
using COG.UI.CustomOption;
using COG.Utils;
using InnerNet;

namespace COG.Listener.Impl;

public class PlayerListener : IListener
{
    public void OnCreatePlayer(AmongUsClient amongUsClient, ClientData client)
    {

        var thread = new Thread(() =>
        {
            Thread.Sleep(500);
            var target = PlayerUtils.GetPlayerById(client.Character.PlayerId);
            if (target == null || target.IsSamePlayer(PlayerControl.LocalPlayer))
            {
                return;
            }
            Main.Logger.LogInfo("Sent options info to " + target.Data.PlayerName);
            CustomOption.ShareConfigs(target);
        });
        thread.Start();
    }
}