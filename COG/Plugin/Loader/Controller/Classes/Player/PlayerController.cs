using System;
using System.Diagnostics.CodeAnalysis;
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

    public COG.Role.Role? GetRoleByPlayer(PlayerControl playerControl)
    {
        return playerControl.GetRoleInstance();
    }

    public void KillPlayer(PlayerControl player)
    {
        player.MurderPlayer(player, GameUtils.DefaultFlag);
    }

    public bool IsRole(PlayerControl player, COG.Role.Role role)
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