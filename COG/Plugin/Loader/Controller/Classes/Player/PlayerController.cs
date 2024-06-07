using System;
using System.Diagnostics.CodeAnalysis;
using COG.Role;
using COG.Utils;
using NLua;

// ReSharper disable UnusedMember.Global

namespace COG.Plugin.Loader.Controller.Classes.Player;

[SuppressMessage("Performance", "CA1822:将成员标记为 static")]
public class PlayerController
{
    private readonly Lua _lua;
    private readonly IPlugin _plugin;

    public PlayerController(Lua lua, IPlugin plugin)
    {
        _lua = lua;
        _plugin = plugin;
    }

    public CustomRole? GetRoleByPlayer(PlayerControl playerControl)
    {
        return playerControl.GetMainRole();
    }

    public void KillPlayer(PlayerControl player)
    {
        player.MurderPlayer(player, GameUtils.DefaultFlag);
    }

    public PlayerControl GetLocalPlayerController()
    {
        return PlayerControl.LocalPlayer;
    }

    public bool IsRole(PlayerControl player, CustomRole role)
    {
        return player.IsRole(role);
    }

    public PlayerControl GetRandomPlayer()
    {
        var random = new Random();
        var players = PlayerUtils.GetAllPlayers();
        return players[random.Next(0, players.Count - 1)];
    }
}