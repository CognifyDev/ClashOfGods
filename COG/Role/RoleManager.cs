using System.Collections.Generic;
using COG.Exception;
using COG.Listener;
using COG.Role.Impl;
using COG.Utils;

namespace COG.Role;

public class RoleManager
{
    private static readonly RoleManager Manager = new();

    public Role[] GetTypeCampRoles(CampType campType)
    {
        var list = new List<Role>();
        foreach (var role in _roles)
        {
            if (role.CampType == campType && role.RoleOptions[0].GetBool())
                list.Add(role);
        }

        return list.ToArray();
    }

    private readonly List<Role> _roles = new();

    public void RegisterRole(Role role)
    {
        _roles.Add(role);
    }

    public Role? GetTypeRoleInstance<T>()
    {
        foreach (var role in _roles)
        {
            if (role is T) return role;
        }

        return null;
    }

    public void RegisterRoles(Role[] roles)
    {
        _roles.AddRange(roles);
    }

    public List<Role> GetRoles()
    {
        return _roles;
    }

    public static RoleManager GetManager()
    {
        return Manager;
    }
}