namespace COG.Listener.Event.Impl;

public class VentEvent : Listener.Event.Event
{
    public Vent Vent;
    
    public VentEvent(Vent vent)
    {
        Vent = vent;
    }
}