using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using COG.Listener;
using InnerNet;
using NLua;

namespace COG.Plugin.Loader.Controller.ClassType.Classes.Listener;

public class Listener : IListener
{
    internal static readonly List<Listener> Listeners = new();
    
    /*
     * Listeners here 
     */

    // 0
    public bool OnPlayerMurder(PlayerControl killer, PlayerControl target) 
        => Type != 0 || CancellableCheck(killer, target);
    
    // 1
    public void OnRPCReceived(byte callId, MessageReader reader)
    {
        if (Type != 1) return;
        var function = Lua.GetFunction(FunctionName);
        function.Call(callId, reader);
    }
    
    // 2
    public bool OnHostChat(ChatController controller) 
        => Type != 2 || CancellableCheck(controller);

    // 3
    public bool OnPlayerChat(PlayerControl player, string text) 
        => Type != 3 || CancellableCheck(player, text);

    // 4
    public void OnChatUpdate(ChatController controller)
    {
        if (Type != 4) return;
        var function = Lua.GetFunction(FunctionName);
        function.Call(controller);
    }
    
    // 5
    public void OnCoBegin()
    {
        if (Type != 5) return;
        var function = Lua.GetFunction(FunctionName);
        function.Call();
    }

    // 6
    public void AfterGameEnd(AmongUsClient client, ref EndGameResult endGameResult)
    {
        if (Type != 6) return;
        var function = Lua.GetFunction(FunctionName);
        if (function.Call(client, endGameResult).First() is EndGameResult result)
        {
            endGameResult = result;
        }
    }
    
    // 7
    public void OnGameStart(GameStartManager manager)
    {
        if (Type != 7) return;
        var function = Lua.GetFunction(FunctionName);
        function.Call(manager);
    }

    // 8
    public void OnGameStartWithMovement(GameManager manager)
    {
        if (Type != 8) return;
        var function = Lua.GetFunction(FunctionName);
        function.Call(manager);
    }
    
    // 9
    public bool OnMakePublic(GameStartManager manager) 
        => Type != 9 || CancellableCheck(manager);
    
    // 10
    public void OnPingTrackerUpdate(PingTracker tracker)
    {
        if (Type != 10) return;
        var function = Lua.GetFunction(FunctionName);
        function.Call(tracker);
    }
    
    // 11
    public void AfterSetUpTeamText(IntroCutscene intro)
    {
        if (Type != 11) return;
        var function = Lua.GetFunction(FunctionName);
        function.Call(intro);
    }
    
    // 12
    public void OnPlayerExile(ExileController controller)
    {
        if (Type != 12) return;
        var function = Lua.GetFunction(FunctionName);
        function.Call(controller);
    }
    
    // 13
    public void OnAirshipPlayerExile(AirshipExileController controller)
    {
        if (Type != 13) return;
        var function = Lua.GetFunction(FunctionName);
        function.Call(controller);
    }
    
    // 14
    public void OnPlayerLeft(AmongUsClient client, ClientData data, DisconnectReasons reason)
    {
        if (Type != 14) return;
        var function = Lua.GetFunction(FunctionName);
        function.Call(client, data, reason.ToString());
    }
    
    // 15
    public void OnPlayerJoin(AmongUsClient client, ClientData data)
    {
        if (Type != 15) return;
        var function = Lua.GetFunction(FunctionName);
        function.Call(client, data);
    }
    
    // 16
    public void OnSelectRoles()
    {
        if (Type != 16) return;
        var function = Lua.GetFunction(FunctionName);
        function.Call();
    }
    
    // 17
    public void OnGameJoined(AmongUsClient amongUsClient, string gameCode)
    {
        if (Type != 17) return;
        var function = Lua.GetFunction(FunctionName);
        function.Call(amongUsClient, gameCode);
    }
    
    // 18
    public void OnGameEndSetEverythingUp(EndGameManager manager)
    {
        if (Type != 18) return;
        var function = Lua.GetFunction(FunctionName);
        function.Call(manager);
    }
    
    // 19
    public void OnIGameOptionsExtensionsDisplay(ref string result)
    {
        if (Type != 19) return;
        var function = Lua.GetFunction(FunctionName);
        if (function.Call(result).First() is string resultString)
        {
            result = resultString;
        }
    }
    
    // 20
    public void OnKeyboardJoystickUpdate(KeyboardJoystick keyboardJoystick)
    {
        if (Type != 20) return;
        var function = Lua.GetFunction(FunctionName);
        function.Call(keyboardJoystick);
    }
    
    // 21
    public bool OnPlayerReportDeadBody(PlayerControl playerControl, GameData.PlayerInfo? target)
    {
        Debug.Assert(target != null, nameof(target) + " != null");
        return Type != 21 || CancellableCheck(playerControl, target);
    }

    // 22
    public void AfterPlayerFixedUpdate(PlayerControl player)
    {
        if (Type != 22) return;
        var function = Lua.GetFunction(FunctionName);
        function.Call(player);
    }
    
    // 23
    public bool OnCheckMurder(PlayerControl killer, PlayerControl target)
        => Type != 23 || CancellableCheck(killer, target);
    
    // 24
    public bool OnDeadBodyClick(DeadBody deadBody)
        => Type != 24 || CancellableCheck(deadBody);

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
        Listeners.Add(this);
    }

    public static Listener? GetListener(IPlugin plugin, string functionName, int type) 
        => Listeners.FirstOrDefault(listener => listener.Type == type && listener.Plugin.Equals(plugin) && listener.FunctionName.Equals(functionName));
}