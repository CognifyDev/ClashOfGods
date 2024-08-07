using COG.Role;
using COG.Utils;
using NLua;
using UnityEngine;

namespace COG.Plugin.Loader.Controller.Classes.Role;

public class RoleController
{
    private readonly Lua _luaController;
    private readonly IPlugin _plugin;

    public RoleController(Lua lua, IPlugin plugin)
    {
        _luaController = lua;
        _plugin = plugin;
    }

    public CustomRole StartRoleInstance(string name, string color, int campType, bool showInOptions)
    {
        return new CustomRoleImplement(name, ColorUtils.AsColor(color), (CampType)campType, showInOptions);
    }

    public void RegisterRole(CustomRole role)
    {
        CustomRoleManager.GetManager().RegisterRole(role);
    }

    private class CustomRoleImplement : CustomRole
    {
        public CustomRoleImplement(string name, Color color, CampType campType, bool showInOptions = true) : base(color, campType, showInOptions)
        {
            Name = name;
            Color = color;
            CampType = campType;
            ShowInOptions = showInOptions;
        }

        private new string Name { get; }
        private new Color Color { get; }
        private new CampType CampType { get; }
        private new bool ShowInOptions { get; }

        public override CustomRole NewInstance()
        {
            return new CustomRoleImplement(Name, Color, CampType, ShowInOptions);
        }
    }
}