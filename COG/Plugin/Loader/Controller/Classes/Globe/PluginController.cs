using System.Linq;
using COG.Utils;
using NLua;

namespace COG.Plugin.Loader.Controller.Classes.Globe;

public class PluginController
{
    private readonly Lua _lua;
    private readonly IPlugin _plugin;

    internal PluginController(Lua lua, IPlugin plugin)
    {
        _lua = lua;
        _plugin = plugin;
    }

    public void UnloadCog()
    {
        DestroyableSingleton<OptionsMenuBehaviour>.Instance.Close();
        Main.Instance.Unload();
    }

    public string GetAuthor()
    {
        return _plugin.GetAuthor();
    }

    public string GetVersion()
    {
        return _plugin.GetVersion();
    }

    public string GetName()
    {
        return _plugin.GetName();
    }

    public string GetMainClass()
    {
        return _plugin.GetMainClass();
    }

    public void OnEnable()
    {
        _plugin.OnEnable();
    }

    public void OnDisable()
    {
        _plugin.OnDisable();
    }

    public RpcUtils.RpcWriter GetRpcWriter(string playerId, string callId, string targets)
    {
        return RpcUtils.StartRpcImmediately(PlayerUtils.GetPlayerById(byte.Parse(playerId))!, byte.Parse(callId),
            targets.Equals("")
                ? null
                : targets.Split(",").Where(p => PlayerUtils.GetPlayerById(byte.Parse(p)) != null)
                    .Select(s => PlayerUtils.GetPlayerById(byte.Parse(s))!).ToArray());
    }
}