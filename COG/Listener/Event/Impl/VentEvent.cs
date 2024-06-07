namespace COG.Listener.Event.Impl;

public class VentEvent : Event
{
    public VentEvent(Vent vent)
    {
        Vent = vent;
    }

    public Vent Vent { get; }
}