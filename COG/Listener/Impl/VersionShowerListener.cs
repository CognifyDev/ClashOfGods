using COG.Listener.Event.Impl.AuClient;
using COG.Listener.Event.Impl.Game;
using COG.Utils;
using InnerNet;
using TMPro;
using UnityEngine;

namespace COG.Listener.Impl;

public class VersionShowerListener : IListener
{
    private static readonly string VersionMsg =
        $@"<color=#DD1717>Clash</color> <color=#690B0B>Of</color> <color=#12EC3D>Gods</color> {Main.PluginVersion}";

    [EventHandler(EventHandlerType.Postfix)]
    public void OnAUClientStart(AmongUsClientStartEvent @event)
    {
        var prefab = Object.FindObjectOfType<PingTracker>();
        var modVersionShower = Object.Instantiate(prefab, HudManager.Instance.transform);
        modVersionShower.name = "InGameModVersionShower";
        modVersionShower.transform.localPosition = new(1.35f, 2.8f, 0);
        modVersionShower.DestroyComponent<AspectPosition>();
        modVersionShower.DestroyComponent<PingTracker>();
        modVersionShower.GetComponent<TextMeshPro>().text = VersionMsg;
    }
}