using System.Collections.Generic;
using System.Linq;
using COG.Utils;

namespace COG.Role;

public class RoleManager
{
    private static readonly RoleManager Manager = new();

    private readonly List<Role> _roles = new();

    public Role[] GetTypeCampRoles(CampType campType)
    {
        var list = new List<Role>();
        foreach (var role in _roles)
            if (role.CampType == campType && role.RoleOptions[0].GetBool())
                list.Add(role);

        return list.ToArray();
    }

    public void RegisterRole(Role role)
    {
        _roles.Add(role);
    }

    public Role? GetTypeRoleInstance<T>()
    {
        foreach (var role in _roles)
            if (role is T)
                return role;

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

    public Role? GetRoleByClassName(string name)
    {
        return _roles.FirstOrDefault(role => role.GetType().Name.ToLower().Equals(name));
    }

    /// <summary>
    ///     获取一个新的获取器
    /// </summary>
    /// <returns>获取器实例</returns>
    public RoleGetter NewGetter()
    {
        return new RoleGetter();
    }

    public static RoleManager GetManager()
    {
        return Manager;
    }

    public class RoleGetter : IGetter<Role?>
    {
        private readonly List<Role> _roles = new();
        private int _selection;

        internal RoleGetter()
        {
            foreach (var role in GetManager().GetRoles()
                         .Where(role => role.ShowInOptions && role.RoleOptions[0].GetBool()))
            {
                var times = (int)role.RoleOptions[1].GetFloat();
                for (var i = 0; i < times; i++) _roles.Add(role);
            }

            _roles.Disarrange();
        }

        public Role? GetNext()
        {
            if (_selection >= _roles.Count) return null;
            var toReturn = _roles[_selection];
            _selection++;
            return toReturn;
        }

        public bool HasNext()
        {
            return _selection < _roles.Count;
        }
    }
}