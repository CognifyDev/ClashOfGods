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

    public IGetter<Role> NewGetter()
    {
        return new RoleGetter();
    }

    public class RoleGetter : IGetter<Role>
    {
        private readonly List<Role> Roles = new(GetManager().GetRoles());

        public RoleGetter()
        {
            Roles = Roles.Disarrange();
        }

        public Role GetNext()
        {
            try
            {
                var role = Roles[0];
                Roles.RemoveAt(0);
                return role;
            }
            catch (System.Exception)
            {
                throw new GetterCanNotGetException("找不到指定Role");
            }
        }

        public bool HasNext()
        {
            return Roles.Count != 0;
        }

        public Role GetNextTypeCampRole(CampType campType)
        {
            for (var i = 0; i < Roles.Count; i++)
            {
                if (Roles[i].CampType == campType)
                {
                    var toReturn = Roles[i];
                    Roles.RemoveAt(i);
                    return toReturn;
                }
            }

            throw new GetterCanNotGetException("找不到指定Role");
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