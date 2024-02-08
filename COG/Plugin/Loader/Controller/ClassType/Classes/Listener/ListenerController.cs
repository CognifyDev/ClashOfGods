using COG.Listener;
using NLua;

namespace COG.Plugin.Loader.Controller.ClassType.Classes.Listener;

public class ListenerController
{
    private Lua Lua { get; }
    private readonly IPlugin _plugin;

    internal ListenerController(Lua lua, IPlugin plugin)
    {
        Lua = lua;
        _plugin = plugin;
    }

    public void RegisterListener(string functionName, int listenerType)
    {
        ListenerManager.GetManager().RegisterListener(new Classes.Listener.Listener(Lua, _plugin, functionName, listenerType));
    }

    public void UnRegisterListener(string functionName, int listenerType)
    {
        var listener = Classes.Listener.Listener.GetListener(_plugin, functionName, listenerType);
        if (listener == null)
        {
            return;
        }

        Classes.Listener.Listener.Listeners.Remove(listener);
        ListenerManager.GetManager().UnregisterListener(listener);
    }
}