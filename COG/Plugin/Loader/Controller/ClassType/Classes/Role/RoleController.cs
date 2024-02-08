using COG.Role;
using COG.Utils;
using NLua;

namespace COG.Plugin.Loader.Controller.ClassType.Classes.Role;

public class RoleController
{
    public Lua Lua { get; }
    public IPlugin Plugin { get; }
    
    public RoleController(Lua lua, IPlugin plugin)
    {
        Lua = lua;
        Plugin = plugin;
    }

    public COG.Role.Role RegisterRole(string name, string color, int campType, bool showInOptions)
    {

        var role = new RolePlugin(name, ColorUtils.AsColor(color), (CampType)campType, showInOptions);
        COG.Role.RoleManager.GetManager().RegisterRole(role);
        return role;
    }

    public COG.Role.RoleManager GetRoleManager() => COG.Role.RoleManager.GetManager();
}