namespace COG.Listener.Event.Impl;

public class ControllerEvent : Listener.Event.Event
{
    public ControllerManager Manager;
    
    public ControllerEvent(ControllerManager manager)
    {
        Manager = manager;
    }
}