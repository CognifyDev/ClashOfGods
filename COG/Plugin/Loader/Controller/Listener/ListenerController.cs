using COG.Utils.Coding;
using NLua;

namespace COG.Plugin.Loader.Controller.Listener;

public class ListenerController
{
    public Lua Lua { get; }
    public IPlugin Plugin { get; }
    
    public ListenerController(Lua lua, IPlugin plugin)
    {
        Lua = lua;
        Plugin = plugin;
    }

    /// <summary>
    /// 注册一个监听器
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="methodName"></param>
    [Unfinished]
    public void RegisterListener(int eventId, string methodName)
    {
        var function = Lua.GetFunction(methodName);
        
    }
}