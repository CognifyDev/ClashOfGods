using COG.Role;
using COG.Utils;
using NLua;

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

    public COG.Role.Role StartRoleInstance(string name, string color, int campType, bool showInOptions)
    {
        return new COG.Role.Role(name, ColorUtils.AsColor(color), (CampType) campType, showInOptions);
    }

    public void RegisterRole(COG.Role.Role role)
    {
        COG.Role.RoleManager.GetManager().RegisterRole(role);
    }
}