using System.Linq;
using COG.Listener;
using NLua;

namespace COG.Plugin.Loader.Controller.Classes.Listener;

public class Listener : IListener
{
    // TODO 监听器

    private bool CancellableCheck(params object[] args)
    {
        var function = Lua.GetFunction(FunctionName);
        bool cancel;
        var results = function.Call(args);
        if (results.Length > 0)
        {
            var result = results.First();
            cancel = result as bool? ?? true;
        }
        else
        {
            cancel = true;
        }
        return cancel;
    }

    public Lua Lua { get; }
    public int Type { get; }
    public IPlugin Plugin { get; }
    public string FunctionName { get; }
    
    public Listener(Lua lua, IPlugin plugin, string functionName, int type)
    {
        Lua = lua;
        Type = type;
        Plugin = plugin;
        FunctionName = functionName;
    }

    // public static Listener? GetListener(IPlugin plugin, string functionName, int type) 
     //   => Listeners.FirstOrDefault(listener => listener.Type == type && listener.Plugin.Equals(plugin) && listener.FunctionName.Equals(functionName));
}