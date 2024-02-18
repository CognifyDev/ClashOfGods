namespace COG.Listener.Event.Impl;

public class HudManagerEvent : Event
{
    public HudManager Manager { get; }
    
    public HudManagerEvent(HudManager manager)
    {
        Manager = manager;
    }
}