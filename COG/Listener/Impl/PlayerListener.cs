using COG.UI.CustomOption;
using InnerNet;

namespace COG.Listener.Impl;

public class PlayerListener : IListener
{
    public void OnPlayerJoin(AmongUsClient client, ClientData data)
    {
        CustomOption.ShareConfigs(client.PlayerPrefab);
    }
}