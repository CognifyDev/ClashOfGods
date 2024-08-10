using System;
using System.Collections.Generic;
using System.Linq;
using COG.Utils;

namespace COG.Role;

public class CustomRoleManager
{
    private static readonly CustomRoleManager Manager = new();

    private readonly List<CustomRole> _roles = new();

    public CustomRole[] GetTypeCampRoles(CampType campType)
    {
        return _roles.Where(role => role.CampType == campType).ToArray();
    }

    public void RegisterRole(CustomRole role)
    {
        _roles.Add(role);
    }

    public T GetTypeRoleInstance<T>() where T : CustomRole
    {
        return _roles.OfType<T>().FirstOrDefault()!;
    }

    public void RegisterRoles(CustomRole[] roles)
    {
        _roles.AddRange(roles);
    }

    public List<CustomRole> GetRoles()
    {
        return _roles;
    }

    public CustomRole? GetRoleByClassName(string name)
    {
        return _roles.FirstOrDefault(r => r.GetType().Name == name);
    }

    public CustomRole? GetRoleById(int id)
    {
        return _roles.FirstOrDefault(role => role.Id == id);
    }

    /// <summary>
    ///     获取一个新的获取器
    /// </summary>
    /// <returns>获取器实例</returns>
    public IGetter<CustomRole> NewGetter(Func<CustomRole, bool> predicate, CustomRole? defaultRole = null)
    {
        return new RoleGetter(predicate, defaultRole);
    }

    public static CustomRoleManager GetManager()
    {
        return Manager;
    }

    public void ReloadRoles()
    {
        var newInstanceRoles = _roles.Select(customRole => customRole.NewInstance()).ToList();
        _roles.Clear();
        _roles.AddRange(newInstanceRoles);
    }

    private class RoleGetter : IGetter<CustomRole>
    {
        private List<CustomRole> CustomRoles { get; } = new();

        private readonly CustomRole? _defaultRole;
        
        public RoleGetter(Func<CustomRole, bool> predicate, CustomRole? defaultRole = null)
        {
            _defaultRole = defaultRole;
            foreach (var role in GetManager().GetRoles().Where(role => role.IsAvailable()))
            {
                if (!predicate(role)) continue;
                if (role.RoleNumberOption == null) continue;
                
                var length = role.RoleNumberOption.GetInt();

                for (var i = 0; i < length; i++)
                {
                    CustomRoles.Add(role);
                }
            }

            CustomRoles = CustomRoles.Disarrange();
        }
        
        public CustomRole GetNext()
        {
            if (!HasNext() && _defaultRole != null)
            {
                return _defaultRole;
            }
            var role = CustomRoles[0];
            CustomRoles.Remove(role);
            return role;
        }

        public bool HasNext()
        {
            return !CustomRoles.IsEmpty();
        }

        public int Number()
        {
            return CustomRoles.Count;
        }

        public void PutBack(CustomRole value)
        {
            CustomRoles.Add(value);
        }
    }
}