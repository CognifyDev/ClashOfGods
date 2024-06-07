namespace COG.Listener.Event.Impl;

public class RoleManagerEvent : Event
{
    public RoleManagerEvent(RoleManager manager)
    {
        RoleManager = manager;
    }

    public RoleManager RoleManager { get; }
}