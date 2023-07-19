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
            if (role.CampType == campType)
                list.Add(role);
        }

        return list.ToArray();
    }

    public Role? GetDefaultRole(CampType campType)
    {
        switch (campType)
        {
            case CampType.Crewmate:
            case CampType.Unknown:
            case CampType.Neutral:
                return GetTypeRoleInstance<Crewmate>();
            case CampType.Impostor:
                return GetTypeRoleInstance<Impostor>();
            default:
                return GetTypeRoleInstance<Crewmate>();
        }
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

    public IGetter<Role?> NewGetter()
    {
        return new RoleGetter();
    }

    public class RoleGetter : IGetter<Role?>
    {
        private readonly List<Role> _roles = new();

        public RoleGetter()
        {
            var roles = GetManager().GetRoles();
            foreach (var role in roles)
            {
                var num = (int)role.RoleOptions[1].GetFloat();
                for (var i = 0; i < num; i++)
                {
                    _roles.Add(role);
                }
            }
            _roles = _roles.Disarrange();
        }

        public Role? GetNext()
        {
            var role = _roles[0];
            _roles.RemoveAt(0);
            if (!role.RoleOptions[0].GetBool())
            {
                return HasNext() ? GetNext() : null;
            }
            return role;
        }

        public bool HasNext()
        {
            return _roles.Count != 0;
        }

        public Role? GetNextTypeCampRole(CampType campType)
        {
            start:
            var role = GetNext();
            if (role == null) return null;
            if (role.CampType != campType) goto start;
            return role;
        }
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