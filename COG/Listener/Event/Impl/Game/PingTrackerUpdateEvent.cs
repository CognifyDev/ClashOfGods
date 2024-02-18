namespace COG.Listener.Event.Impl.Game;

public class PingTrackerUpdateEvent : GameEvent<PingTracker>
{
    public PingTrackerUpdateEvent(PingTracker obj) : base(obj)
    {
    }
}