using System;
using System.Collections.Generic;
using System.Linq;
using COG.Config.Impl;
using COG.Listener;
using COG.Role;
using COG.Role.Impl;
using InnerNet;
using UnityEngine;
using GameStates = COG.States.GameStates;
using Object = UnityEngine.Object;

namespace COG.Utils;

public enum ColorType
{
    Unknown = -1,
    Light,
    Dark
}

public static class PlayerUtils
{
    internal static readonly List<PlayerControl> Players = new();

    public static List<KeyValuePair<PlayerControl, Role.Role>> AllImpostors =>
        GameUtils.Data.Where(pair => pair.Value.CampType == CampType.Impostor).ToListCustom();

    public static List<KeyValuePair<PlayerControl, Role.Role>> AllCremates =>
        GameUtils.Data.Where(pair => pair.Value.CampType == CampType.Crewmate).ToListCustom();

    public static List<KeyValuePair<PlayerControl, Role.Role>> AllNeutrals =>
        GameUtils.Data.Where(pair => pair.Value.CampType == CampType.Neutral).ToListCustom();

    public static List<PlayerControl> GetAllPlayers()
    {
        return Players.ToArray().Where(player => player != null).ToListCustom();
    }

    /// <summary>
    /// 获取距离目标玩家位置最近的玩家
    /// </summary>
    /// <param name="target">目标玩家</param>
    /// <param name="mustAlive">是否必须为活着的玩家</param>
    /// <returns>最近位置的玩家</returns>
    public static PlayerControl? GetClosestPlayer(this PlayerControl target, bool mustAlive = true)
    {
        var targetLocation = target.GetTruePosition();
        var players = mustAlive ? GetAllAlivePlayers() : GetAllPlayers();

        PlayerControl? closestPlayer = null;
        var closestDistance = float.MaxValue;

        foreach (var player in players)
        {
            if (player == target) continue;

            var playerLocation = player.GetTruePosition();
            var distance = Vector2.Distance(targetLocation, playerLocation);

            if (!(distance < closestDistance)) continue;
            closestDistance = distance;
            closestPlayer = player;
        }

        return closestPlayer;
    }


    public static List<PlayerControl> GetAllAlivePlayers()
    {
        return Players.ToArray().Where(player => player != null && player.IsAlive()).ToListCustom();
    }

    public static bool IsSamePlayer(this GameData.PlayerInfo info, GameData.PlayerInfo target)
    {
        return info.FriendCode.Equals(target.FriendCode) && info.PlayerName.Equals(target.PlayerName);
    }

    public static bool IsSamePlayer(this PlayerControl player, PlayerControl target)
    {
        return player.name.Equals(target.name) && player.FriendCode.Equals(target.FriendCode);
    }

    public static Role.Role? GetRoleInstance(this PlayerControl player)
    {
        return (from keyValuePair in GameUtils.Data
                where keyValuePair.Key.IsSamePlayer(player)
                select keyValuePair.Value)
            .FirstOrDefault();
    }

    public static void SetNamePrivately(PlayerControl target, PlayerControl seer, string name)
    {
        var writer =
            AmongUsClient.Instance.StartRpcImmediately(target.NetId, (byte)RpcCalls.SetName, SendOption.Reliable,
                seer.GetClientID());
        writer.Write(name);
        writer.Write(false);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    public static PlayerControl? GetPlayerById(byte playerId)
    {
        foreach (var playerControl in GetAllPlayers())
            if (playerControl.PlayerId == playerId)
                return playerControl;

        return null;
    }

    public static ClientData? GetClient(this PlayerControl player)
    {
        var client = AmongUsClient.Instance.allClients.ToArray()
            .FirstOrDefault(cd => cd.Character.PlayerId == player.PlayerId);
        return client;
    }

    public static int GetClientID(this PlayerControl player)
    {
        return player.GetClient() == null ? -1 : player.GetClient()!.Id;
    }

    /// <summary>
    ///     检测玩家是否存活
    /// </summary>
    /// <param name="player">玩家实例</param>
    /// <returns></returns>
    public static bool IsAlive(this PlayerControl player)
    {
        if (GameStates.IsLobby) return true;
        if (player == null) return false;
        return !player.Data.IsDead;
    }

    public static bool IsRole(this PlayerControl player, Role.Role role)
    {
        return player.GetRoleInstance() == role;
    }

    public static DeadBody? GetClosestBody(List<DeadBody> untargetable)
    {
        DeadBody? result = null;

        var num = PlayerControl.LocalPlayer.MaxReportDistance;
        if (!ShipStatus.Instance) return null;
        var position = PlayerControl.LocalPlayer.GetTruePosition();

        foreach (var body in Object.FindObjectsOfType<DeadBody>().Where(b => !untargetable.Contains(b)))
        {
            var vector = body.TruePosition - position;
            var magnitude = vector.magnitude;
            if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(position, vector.normalized, magnitude,
                    Constants.ShipAndObjectsMask))
            {
                result = body;
                num = magnitude;
            }
        }

        return result;
    }

    public static ColorType GetColorType(this PlayerControl player)
    {
        return player.cosmetics.ColorId switch
        {
            0 or 3 or 4 or 5 or 7 or 10 or 11 or 13 or 14 or 17 => ColorType.Light,
            1 or 2 or 6 or 8 or 9 or 12 or 15 or 16 or 18 => ColorType.Dark,
            _ => ColorType.Unknown
        };
    }

    public static string GetLanguageDeathReason(this DeathReason? deathReason)
    {
        return deathReason switch
        {
            DeathReason.Default => LanguageConfig.Instance.DefaultKillReason,
            DeathReason.Disconnected => LanguageConfig.Instance.Disconnected,
            _ => LanguageConfig.Instance.UnknownKillReason
        };
    }
}

public enum DeathReason
{
    Unknown = -1,
    Disconnected,
    Default,
    Exiled
}

public class DeadPlayerManager : IListener
{
    public static List<DeadPlayer> DeadPlayers { get; } = new();

    public void OnMurderPlayer(PlayerControl killer, PlayerControl target)
    {
        if (!(target.Data.IsDead && killer && target)) return;

        var reason = GetDeathReason(killer, target);
        _ = new DeadPlayer(DateTime.Now, reason, target, killer);
    }

    public void OnPlayerLeft(AmongUsClient client, ClientData data, DisconnectReasons reason)
    {
        if (DeadPlayers.Any(dp => dp.Player.NetId == data.Character.NetId)) return;
        _ = new DeadPlayer(DateTime.Now, DeathReason.Disconnected, data.Character, null);
    }

    public void OnCoBegin()
    {
        DeadPlayers.Clear();
    }

    public void OnPlayerExile(ExileController controller)
    {
        if (controller.exiled == null) return;
        _ = new DeadPlayer(DateTime.Now, DeathReason.Exiled, controller.exiled.Object, null);
    }

    public void OnAirshipPlayerExile(AirshipExileController controller)
    {
        OnPlayerExile(controller);
    }

    public static DeathReason GetDeathReason(PlayerControl killer, PlayerControl target)
    {
        try
        {
            return DeathReason.Default;
        }
        catch
        {
            return DeathReason.Unknown;
        }
    }

    public class DeadPlayer
    {
        public DeadPlayer(DateTime deadTime, DeathReason? deathReason, PlayerControl player, PlayerControl? killer)
        {
            DeadTime = deadTime;
            DeathReason = deathReason;
            Player = player;
            Killer = killer;
            Role = player.GetRoleInstance();
            PlayerId = player.PlayerId;
            DeadPlayers.Add(this);
        }

        public DateTime DeadTime { get; private set; }
        public DeathReason? DeathReason { get; }
        public PlayerControl Player { get; }
        public PlayerControl? Killer { get; }
        public Role.Role? Role { get; private set; }
        public byte PlayerId { get; }

        //先这样，以后再改，反正暂时用不着
        public override string ToString()
        {
            return Player + " was killed by " + Killer;
        }
    }
}

public class PlayerRole
{
    public PlayerRole(PlayerControl player, Role.Role role, string name)
    {
        Player = player;
        Role = role;
        PlayerName = name;
        PlayerId = player.PlayerId;
        CachedRoles.Add(this);
    }

    public static List<PlayerRole> CachedRoles { get; set; } = new();
    public PlayerControl Player { get; private set; }
    public Role.Role Role { get; }
    public string PlayerName { get; }
    public byte PlayerId { get; }

    public static Role.Role GetRole(string? playerName = null, byte? playerId = null)
    {
        return CachedRoles.FirstOrDefault(pr => pr.PlayerName == playerName || pr.PlayerId == playerId) != null
            ? CachedRoles.FirstOrDefault(pr => pr.PlayerName == playerName || pr.PlayerId == playerId)!.Role
            : COG.Role.RoleManager.GetManager().GetTypeRoleInstance<Unknown>();
    }
}

public class CachedPlayer : IListener
{
    public CachedPlayer(PlayerControl player)
    {
        if (!player) return;

        Player = player;
        PlayerName = player.Data.PlayerName;
        PlayerId = player.PlayerId;
        ColorId = player.cosmetics.ColorId;
        FriendCode = player.FriendCode;

        AllPlayers.Add(this);
    }

    public CachedPlayer()
    {
    } // For registering listeners

    public static List<CachedPlayer> AllPlayers { get; } = new();

    public PlayerControl? Player { get; }

    public Role.Role MyRole => PlayerRole.CachedRoles.FirstOrDefault(dp => dp.PlayerId == PlayerId)?.Role ??
                               Role.RoleManager.GetManager().GetTypeRoleInstance<Unknown>();

    public string? PlayerName { get; }
    public byte PlayerId { get; }
    public int ColorId { get; }
    public string? FriendCode { get; private set; }

    public DeadPlayerManager.DeadPlayer? DeadStatus =>
        DeadPlayerManager.DeadPlayers.FirstOrDefault(dp => dp.PlayerId == PlayerId);

    public bool IsDead => DeadStatus == null;
    public bool PlayerIsNull => Player == null;

    public void OnPlayerJoin(AmongUsClient amongUsClient, ClientData data)
    {
        _ = new CachedPlayer(data.Character);
    }

    public void OnCoBegin()
    {
        AllPlayers.RemoveAll(cp =>
            cp.IsDead && /* Will not continue if cp.IsDead is true */
            cp.DeadStatus!.DeathReason == DeathReason.Disconnected);
    }

    public void OnGameJoined(AmongUsClient amongUsClient, string gameIdString)
    {
        AllPlayers.Clear();
        // Reset
    }

    public static CachedPlayer? FindPlayer(PlayerControl? player = null, string? name = null, byte? playerId = null,
        int colorId = -1)
    {
        foreach (var cp in AllPlayers)
            if (
                (player && cp.Player == player)
                || cp.PlayerName == name
                || cp.PlayerId == playerId
                || colorId == cp.ColorId
            )
                return cp;
        return null;
    }

    public static implicit operator bool(CachedPlayer? player)
    {
        return player != null;
    }
}