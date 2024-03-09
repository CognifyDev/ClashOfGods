namespace COG.Listener.Event.Impl;

public class RoleManagerEvent : Event
{
    public RoleManager RoleManager { get; }

    public RoleManagerEvent(RoleManager manager)
    {
        RoleManager = manager;
    }
}