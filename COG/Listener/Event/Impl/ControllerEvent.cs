namespace COG.Listener.Event.Impl;

public class ControllerEvent : Event
{
    public ControllerEvent(ControllerManager manager)
    {
        Manager = manager;
    }

    public ControllerManager Manager { get; }
}