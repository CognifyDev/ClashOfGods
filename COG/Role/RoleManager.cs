using System.Collections.Generic;
using System.Linq;
using COG.Utils;

namespace COG.Role;

public class RoleManager
{
    private static readonly RoleManager Manager = new();

    private readonly List<Role> _roles = new();

    private uint _nextId = 0;

    public Role[] GetTypeCampRoles(CampType campType)
    {
        return _roles.Where(role => role.CampType == campType && role.MainRoleOption!.GetBool()).ToArray();
    }

    public void RegisterRole(Role role)
    {
        _roles.Add(role);
    }

    public Role GetTypeRoleInstance<T>() where T : Role
    {
        return _roles.OfType<T>().FirstOrDefault()!;
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
        return _roles.FirstOrDefault(r => r.GetType().Name == name);
    }

    public Role? GetRoleById(uint id)
    {
        return _roles.FirstOrDefault(role => role.Id == id);
    }

    public uint GetAvailableRoleId() => _nextId++;

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
                         .Where(role =>
                             role.MainRoleOption != null && role.ShowInOptions && role.MainRoleOption.GetBool()))
                if (role.RoleNumberOption != null)
                {
                    var times = (int)role.RoleNumberOption.GetFloat();
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