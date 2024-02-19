namespace COG.Listener.Event.Impl;

public class ControllerEvent : Event
{
    public ControllerManager Manager;

    public ControllerEvent(ControllerManager manager)
    {
        Manager = manager;
    }
}