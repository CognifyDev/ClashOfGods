namespace COG.Listener.Event.Impl.RManager;

public class RoleManagerSelectRolesEvent : RoleManagerEvent
{
    public RoleManagerSelectRolesEvent(RoleManager manager) : base(manager)
    {
        RoleManager = manager;
    }

    public new RoleManager RoleManager { get; }
}