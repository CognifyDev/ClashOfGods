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
        return _roles.Where(role => role.CampType == campType && role.MainRoleOption!.GetBool()).ToArray();
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
    public RoleGetter NewGetter(bool subRolesOnly = false)
    {
        return new RoleGetter(subRolesOnly);
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

    public class RoleGetter : IGetter<CustomRole?>
    {
        private readonly List<CustomRole> _roles = new();
        private int _selection;

        internal RoleGetter(bool subRolesOnly = false)
        {
            foreach (var role in GetManager().GetRoles()
                         .Where(role =>
                             role is { MainRoleOption: not null, ShowInOptions: true, IsBaseRole: false } &&
                             role.MainRoleOption.GetBool() && role.IsSubRole == subRolesOnly))
                if (!role.OnRoleSelection(_roles) && role.RoleNumberOption != null)
                {
                    var times = (int)role.RoleNumberOption.GetFloat();
                    for (var i = 0; i < times; i++) _roles.Add(role);
                }

            _roles.Disarrange();
        }

        public CustomRole? GetNext()
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

        public bool IsEmpty()
        {
            return _roles.IsEmpty();
        }
    }
}