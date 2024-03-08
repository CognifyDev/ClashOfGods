namespace COG.Listener.Event.Impl;

public class VentEvent : Event
{
    public Vent Vent { get; }

    public VentEvent(Vent vent)
    {
        Vent = vent;
    }
}