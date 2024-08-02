using COG.Listener.Event.Impl.Game;
using COG.Utils;
using TMPro;
using UnityEngine;

namespace COG.Listener.Impl;

public class VersionShowerListener : IListener
{
    public const string ShowerObjectName = "InGameModVersionShower";

    public static readonly string VersionMsg =
            $"<color=#DD1717>Clash</color> <color=#690B0B>Of</color> <color=#12EC3D>Gods</color> {Main.PluginVersion}\n"
#if DEBUG
            + $"{GitInfo.Branch} ({GitInfo.Commit} at {Main.CommitTime})"
#endif
        ;

    public bool HasCreatedShower => HudManager.Instance.transform.Find(ShowerObjectName);

    [EventHandler(EventHandlerType.Postfix)]
    public void OnPingTrackerUpdate(PingTrackerUpdateEvent @event)
    {
        if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) return;
        if (HasCreatedShower) return;
        var prefab = Object.FindObjectOfType<PingTracker>();
        var modVersionShower = Object.Instantiate(prefab, HudManager.Instance.transform);
        modVersionShower.name = ShowerObjectName;
        modVersionShower.transform.localPosition = new Vector3(1.35f, 2.8f, 0);
        modVersionShower.DestroyComponent<AspectPosition>();
        modVersionShower.DestroyComponent<PingTracker>();
        var tmp = modVersionShower.GetComponent<TextMeshPro>();
        tmp.text = VersionMsg;
        tmp.alignment = TextAlignmentOptions.TopRight;
    }
}