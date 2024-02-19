namespace COG.Listener.Event.Impl;

public class VentEvent : Event
{
    public Vent Vent;

    public VentEvent(Vent vent)
    {
        Vent = vent;
    }
}