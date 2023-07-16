using System.Collections.Generic;

namespace COG.Role;

public class RoleManager
{
    private static readonly RoleManager Manager = new();

    public Role GetDefaultRole(Camp camp)
    {
        switch (camp)
        {
            case Camp.Neutral:
            case Camp.Unknown:
            case Camp.Crewmate: 
                return Roles[0];
            default:
                return Roles[0];
            case Camp.Impostor:
                return Roles[1];
        }
    }

    private readonly List<Role> Roles = new();

    public void RegisterRole(Role role)
    {
        Roles.Add(role);
    }

    public void RegisterRoles(Role[] roles)
    {
        Roles.AddRange(roles);
    }

    public List<Role> GetRoles()
    {
        return Roles;
    }

    public static RoleManager GetManager()
    {
        return Manager;
    }
}