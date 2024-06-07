namespace COG.Listener.Event.Impl;

public class HudManagerEvent : Event
{
    public HudManagerEvent(HudManager manager)
    {
        Manager = manager;
    }

    public HudManager Manager { get; }
}