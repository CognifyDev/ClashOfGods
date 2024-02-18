namespace COG.Listener.Event.Impl.RManager;

public class RoleManagerSelectRolesEvent : RoleManagerEvent
{
    public RoleManager RoleManager { get; }
    
    public RoleManagerSelectRolesEvent(RoleManager manager) : base(manager)
    {
        RoleManager = manager;
    }
}