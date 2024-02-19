using COG.Listener.Event.Impl.Game;
using InnerNet;
using TMPro;
using UnityEngine;

namespace COG.Listener.Impl;

public class VersionShowerListener : IListener
{
    private static readonly string VersionMsg =
        $@"<color=#DD1717>Clash</color> <color=#690B0B>Of</color> <color=#12EC3D>Gods</color> {Main.PluginVersion}";

    // The method of show version from SNR(Super New Roles)
    [EventHandler(EventHandlerType.Postfix)]
    public void OnPingTrackerUpdate(PingTrackerUpdateEvent @event)
    {
        var tracker = @event.Object;
        tracker.text.alignment = TextAlignmentOptions.TopRight;
        if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started)
        {
            tracker.text.text = $"{VersionMsg}\n{tracker.text.text}";
            tracker.gameObject.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(1.2f, 0.1f, 0.5f);
        }
        else
        {
            tracker.text.text = $"{VersionMsg}\n{tracker.text.text}";
            var transform = tracker.transform;
            var localPosition = transform.localPosition;
            localPosition = new Vector3(4f, localPosition.y, localPosition.z);
            transform.localPosition = localPosition;
        }
    }
}