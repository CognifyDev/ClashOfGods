using System.Collections.Generic;
using System.Linq;
using COG.Listener;
using COG.Role.Impl.Crewmate;
using COG.Role;
using InnerNet;
using UnityEngine;
using GameStates = COG.States.GameStates;
using System;

namespace COG.Utils;

public enum ColorType
{
    Unknown = -1,
    Light,
    Dark,
}

public static class PlayerUtils
{
    public static List<PlayerControl> GetAllPlayers()
    {
        return PlayerControl.AllPlayerControls.ToArray().Where(player => player != null).ToList();
    }

    public static List<PlayerControl> GetAllAlivePlayers()
    {
        return PlayerControl.AllPlayerControls.ToArray().Where(player => player != null && player.IsAlive()).ToList();
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

    public static bool IsRole(this PlayerControl player, Role.Role role) => player.GetRoleInstance() == role;

    public static DeadBody? GetClosestBody(List<DeadBody> untargetable)
    {
        DeadBody? result = null;

        float num = PlayerControl.LocalPlayer.MaxReportDistance;
        if (!ShipStatus.Instance) return null;
        var position = PlayerControl.LocalPlayer.GetTruePosition();

        foreach (var body in GameObject.FindObjectsOfType<DeadBody>().Where(b => !untargetable.Contains(b)))
        {
            var vector = body.TruePosition - position;
            float magnitude = vector.magnitude;
            if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(position, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
            {
                result = body;
                num = magnitude;
            }
        }
        return result;
    }

    public static ColorType GetColorType(this PlayerControl player) => player.cosmetics.ColorId switch
    {
        0 or 3 or 4 or 5 or 7 or 10 or 11 or 13 or 14 or 17 => ColorType.Light,
        1 or 2 or 6 or 8 or 9 or 12 or 15 or 16 or 18 => ColorType.Dark,
        _ => ColorType.Unknown
    };
}




public enum DeathReason
{
    Unknown = -1,
    Default,
    Misfire,
    BySheriffKill,
}

public class DeadPlayerManager : IListener
{
    public static List<DeadPlayer> DeadPlayers { get; private set; } = new();

    public class DeadPlayer
    {
        public DateTime DeadTime { get; private set; }
        public DeathReason? DeathReason { get; private set; }
        public PlayerControl Player { get; private set; }
        public PlayerControl Killer { get; private set; }
        public Role.Role? Role { get; private set; }
        public DeadPlayer(DateTime deadTime, DeathReason? deathReason, PlayerControl player, PlayerControl killer)
        {
            DeadTime = deadTime;
            DeathReason = deathReason;
            Player = player;
            Killer = killer;
            Role = player.GetRoleInstance();
            DeadPlayers.Add(this);
        }

        //先这样，以后再改，反正暂时用不着
        public override string ToString() => Player + " was killed by " + Killer;
    }

    public void OnMurderPlayer(PlayerControl killer, PlayerControl target)
    {
        if (!(target.Data.IsDead && killer && target)) return;

        var reason = GetDeathReason(killer, target);
        new DeadPlayer(DateTime.Now, reason, target, killer);
    }
    public void OnCoBegin() => DeadPlayers.Clear();
    

    private static DeathReason GetDeathReason(PlayerControl killer, PlayerControl target)
    {
        try
        {
            if (killer == target && killer.IsRole(Sheriff.Instance)) return DeathReason.Misfire;
            if (killer != target && killer.IsRole(Sheriff.Instance) && target.GetRoleInstance()!.CampType != CampType.Crewmate) return DeathReason.BySheriffKill;
            return DeathReason.Default;
        }
        catch { return DeathReason.Unknown; }
    }
}