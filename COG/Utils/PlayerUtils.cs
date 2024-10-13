using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AmongUs.Data;
using COG.Config.Impl;
using COG.Listener;
using COG.Listener.Event.Impl.AuClient;
using COG.Listener.Event.Impl.Player;
using COG.Role;
using COG.Role.Impl;
using COG.Rpc;
using InnerNet;
using UnityEngine;
using GameStates = COG.States.GameStates;


namespace COG.Utils;

public enum ColorType
{
    Unknown = -1,
    Light,
    Dark
}

public static class PlayerUtils
{
    public static readonly int Outline = Shader.PropertyToID("_Outline");
    public static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
    public static PoolablePlayer? PoolablePlayerPrefab { get; set; }

    public static IEnumerable<PlayerData> AllImpostors =>
        GameUtils.PlayerData.Where(pair => pair.Player && pair.MainRole.CampType == CampType.Impostor);

    public static IEnumerable<PlayerData> AllCrewmates =>
        GameUtils.PlayerData.Where(pair => pair.Player && pair.MainRole.CampType == CampType.Crewmate);

    public static IEnumerable<PlayerData> AllNeutrals =>
        GameUtils.PlayerData.Where(pair => pair.Player && pair.MainRole.CampType == CampType.Neutral);

    /// <summary>
    ///     获取距离目标玩家位置最近的玩家
    /// </summary>
    /// <param name="target">目标玩家</param>
    /// <param name="mustAlive">是否必须为活着的玩家</param>
    /// <param name="closestDistance">限制距离</param>
    /// <returns>最近位置的玩家</returns>
    public static PlayerControl? GetClosestPlayer(this PlayerControl target, bool mustAlive = true,
        float closestDistance = float.MaxValue)
    {
        var targetLocation = target.GetTruePosition();
        var players = mustAlive ? GetAllAlivePlayers() : GetAllPlayers();

        PlayerControl? closestPlayer = null;

        foreach (var player in players)
        {
            if (player == target) continue;

            var playerLocation = player.GetTruePosition();
            var distance = Vector2.Distance(targetLocation, playerLocation);

            if (distance >= closestDistance) continue;
            closestDistance = distance;
            closestPlayer = player;
        }

        return closestPlayer;
    }
    
    public const MurderResultFlags SucceededFlags = MurderResultFlags.Succeeded | MurderResultFlags.DecisionByHost;

    public static void RpcAdvancedMurderPlayer(this PlayerControl killer, PlayerControl target)
    {
        if (target == null)
        {
            target = killer;
        }
        
        if (AmongUsClient.Instance.AmClient)
        {
            killer.MurderPlayer(target, SucceededFlags);
        }
        var messageWriter = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)RpcCalls.MurderPlayer, SendOption.None);
        messageWriter.WriteNetObject(target);
        messageWriter.Write((int) SucceededFlags);
        AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
    }

    /// <summary>
    /// 杀死一个玩家不留下鸡腿
    /// </summary>
    /// <param name="killer"></param>
    /// <param name="target"></param>
    /// <param name="showAnimationToEverybody"></param>
    public static void RpcKillPlayerCompletely(this PlayerControl killer, PlayerControl target, bool showAnimationToEverybody = false)
    {
        KillPlayerCompletely(killer, target, showAnimationToEverybody);

        var rpc = RpcUtils.StartRpcImmediately(PlayerControl.LocalPlayer, KnownRpc.KillPlayerCompletely);
        rpc.WriteNetObject(killer);
        rpc.WriteNetObject(target);
        rpc.Write(showAnimationToEverybody);
        rpc.Finish();
    }

    public static void KillPlayerCompletely(this PlayerControl killer, PlayerControl target, bool showAnimationToEverybody = false)
    {
        _ = new DeadPlayer(DateTime.Now, DeathReason.Default, target.Data, killer.Data);
        target.Exiled();
        if (MeetingHud.Instance)
        {
            foreach (var pva in MeetingHud.Instance.playerStates)
            {
                if (pva.TargetPlayerId == target.PlayerId)
                {
                    pva.SetDead(pva.DidReport, true);
                    pva.Overlay.gameObject.SetActive(true);
                }
                if (pva.VotedFor != target.PlayerId) continue;
                pva.UnsetVote();
                if (!target.AmOwner) continue;
                MeetingHud.Instance.ClearVote();
            }
            if (AmongUsClient.Instance.AmHost)
                MeetingHud.Instance.CheckForEndVoting();
        }

        foreach (var player in GetAllAlivePlayers())
        {
            if (player.PlayerId == target.PlayerId)
            {
                HudManager.Instance.KillOverlay.ShowKillAnimation(killer.Data, target.Data);
                continue;
            }

            if (showAnimationToEverybody)
                HudManager.Instance.KillOverlay.ShowKillAnimation(target.Data, target.Data);
        }
    }

    public static List<PlayerControl> GetAllPlayers()
    {
        return new List<PlayerControl>(PlayerControl.AllPlayerControls.ToArray());
    }

    public static List<PlayerControl> GetAllAlivePlayers()
    {
        return GetAllPlayers().ToArray().Where(player => player.IsAlive()).ToList();
    }

    public static bool IsSamePlayer(this NetworkedPlayerInfo info, NetworkedPlayerInfo target)
    {
        return IsSamePlayer(info.Object, target.Object);
    }

    public static bool IsSamePlayer(this PlayerControl? player, PlayerControl? target)
    { 
        if (player == null || target == null) return false; 
        return player.PlayerId == target.PlayerId;
    }

    public static DeadBody? GetDeadBody(this PlayerControl target)
    {
        var deadBodies = DeadBodyUtils.GetDeadBodies().Where(body => 
            body.GetPlayer().IsSamePlayer(target)).ToList();
        return deadBodies.Count > 0 ? deadBodies[0] : null;
    }

    public static PlayerData? GetPlayerData(this PlayerControl player)
    {
        return player.Data.GetPlayerData();
    }

    public static PlayerData? GetPlayerData(this NetworkedPlayerInfo player)
    {
        return GameUtils.PlayerData.FirstOrDefault(playerRole => playerRole.Player.Data.IsSamePlayer(player));
    }

    /// <summary>
    /// 给玩家添加指定标签
    /// </summary>
    /// <param name="tag"></param>
    public static void RpcMark(this PlayerControl target, string tag)
    {
        var playerData = target.GetPlayerData();
        playerData?.Tags.Add(tag);

        var writer = RpcUtils.StartRpcImmediately(PlayerControl.LocalPlayer, KnownRpc.Mark);
        writer.WriteNetObject(target);
        writer.Write(tag);
        writer.Finish();
    }

    public const string DeleteTagPrefix = "DELETE010_TAG101_";

    public static void RemoveMark(this PlayerControl target, string tag)
    {
        var playerData = target.GetPlayerData();
        if (playerData == null)
        {
            return;
        }
        if (!playerData.Tags.Contains(tag))
        {
            return;
        }

        var writer = RpcUtils.StartRpcImmediately(PlayerControl.LocalPlayer, KnownRpc.Mark);
        writer.WriteNetObject(target);
        writer.Write(DeleteTagPrefix + tag);
        writer.Finish();
    }

    public static string[] GetMarks(this PlayerControl target)
    {
        var tags = target.GetPlayerData()?.Tags;
        return tags == null ? Array.Empty<string>() : tags.ToArray();
    }

    public static bool HasMarkAs(this PlayerControl target, string tag)
    {
        var tags = target.GetPlayerData()?.Tags;
        return tags != null && tags.Contains(tag);
    }
    
    /// <summary>
    /// 复活一个玩家
    /// </summary>
    /// <param name="player">欲复活的玩家</param>
    public static void RpcRevive(this PlayerControl player)
    {
        player.Revive();

        var writer = RpcUtils.StartRpcImmediately(PlayerControl.LocalPlayer, KnownRpc.Revive);
        
        // 写入对象实例
        writer.WriteNetObject(player);
        
        // 发送Rpc
        writer.Finish();
    }

    public static CustomRole GetMainRole(this PlayerControl player)
    {
        return GameUtils.PlayerData.FirstOrDefault(d => d.Player.IsSamePlayer(player))?.MainRole ??
               CustomRoleManager.GetManager().GetTypeRoleInstance<Unknown>(); // 一般来说玩家游戏职业不为空
    }

    public static CustomRole GetMainRole(this NetworkedPlayerInfo player)
    {
        return GameUtils.PlayerData.FirstOrDefault(d => d.Data.IsSamePlayer(player))?.MainRole ??
               CustomRoleManager.GetManager().GetTypeRoleInstance<Unknown>();
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

    public static PlayerControl? GetPlayerById(byte? playerId)
    {
        return GetAllPlayers().FirstOrDefault(playerControl => playerControl.PlayerId == playerId);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public static ClientData? GetClient(this PlayerControl player)
    {
        if (!AmongUsClient.Instance)
        {
            return null;
        }
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
        if (GameStates.InLobby) return true;
        if (player == null) return false;
        return !player.Data.IsDead;
    }

    public static bool IsRole(this PlayerControl player, CustomRole role)
    {
        return player.Data?.IsRole(role) ?? false;
    }

    public static bool IsRole(this NetworkedPlayerInfo player, CustomRole role)
    {
        if (!player) return false; // 在对局内只要是游戏正式开始时在游戏内的玩家，玩家的NetworkedPlayerInfo绝对不为空
        
        var targetRole = player.GetPlayerData();
        return targetRole?.MainRole != null && (targetRole.MainRole.Id == role.Id || targetRole.SubRoles.Select(customRole => customRole.Id).Contains(role.Id));
    }

    public static DeadBody? GetClosestBody(List<DeadBody>? unTargetAble = null)
    {
        DeadBody? result = null;

        var num = PlayerControl.LocalPlayer.MaxReportDistance;
        if (!ShipStatus.Instance) return null;
        var position = PlayerControl.LocalPlayer.GetTruePosition();

        foreach (var body in Object.FindObjectsOfType<DeadBody>()
                     .Where(b => unTargetAble?.Contains(b) ?? true))
        {
            var vector = body.TruePosition - position;
            var magnitude = vector.magnitude;
            if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(position, vector.normalized, 
                    magnitude, Constants.ShipAndObjectsMask))
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

    public static void RpcSetNamePrivately(this PlayerControl player, string name, PlayerControl[] targets)
    {
        foreach (var target in targets)
        {
            var clientId = target.GetClientID();
            var writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetName,
                SendOption.Reliable, clientId);
            writer.Write(name);
            writer.Write(false);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }

    /// <summary>
    ///     设置玩家外观
    /// </summary>
    /// <param name="poolable"></param>
    /// <param name="player"></param>
    public static void SetPoolableAppearance(this PlayerControl player, PoolablePlayer poolable)
    {
        if (!poolable || !player) return;

        var outfit = player.CurrentOutfit;
        var data = player.Data;
        poolable.SetBodyColor(player.cosmetics.ColorId);
        if (data.IsDead) poolable.SetBodyAsGhost();
        poolable.SetBodyType(player.BodyType);
        if (DataManager.Settings.Accessibility.ColorBlindMode) poolable.SetColorBlindTag();

        poolable.SetSkin(outfit.SkinId, outfit.ColorId);
        poolable.SetHat(outfit.HatId, outfit.ColorId);
        poolable.SetName(data.PlayerName);
        poolable.SetFlipX(true);
        poolable.SetBodyCosmeticsVisible(true);
        poolable.SetVisor(outfit.VisorId, outfit.ColorId);

        var names = poolable.transform.FindChild("Names");
        names.localPosition = new Vector3(0, -0.75f, 0);
        names.localScale = new Vector3(1.5f, 1.5f, 1f);
    }

    /// <summary>
    ///     设置角色外侧的线
    ///     比如: 击杀时候的红线
    /// </summary>
    /// <param name="pc">目标玩家</param>
    /// <param name="color">颜色</param>
    public static void SetOutline(this PlayerControl pc, Color color)
    {
        pc.cosmetics.currentBodySprite.BodySprite.material.SetFloat(Outline, 1f);
        pc.cosmetics.currentBodySprite.BodySprite.material.SetColor(OutlineColor, color);
    }

    public static void ClearOutline(this PlayerControl pc)
    {
        pc.cosmetics.currentBodySprite.BodySprite.material.SetFloat(Outline, 0);
    }

    public static bool IsRole<T>(this PlayerControl pc) where T : CustomRole
    {
        return IsRole(pc, CustomRoleManager.GetManager().GetTypeRoleInstance<T>());
    }

    public static bool IsRole<T>(this NetworkedPlayerInfo data) where T : CustomRole
    {
        return IsRole(data, CustomRoleManager.GetManager().GetTypeRoleInstance<T>());
    }

    public static PlayerControl? SetClosestPlayerOutline(this PlayerControl pc, Color color, bool checkDist = true)
    {
        var target = pc.GetClosestPlayer();
        PlayerControl.AllPlayerControls.ForEach(new Action<PlayerControl>(p => p.ClearOutline()));
        if (!target) return null;
        if (GameUtils.GetGameOptions().KillDistance >=
            Vector2.Distance(PlayerControl.LocalPlayer.GetTruePosition(),
                target!.GetTruePosition()) && checkDist)
        {
            target.SetOutline(color);
            return target;
        }

        return null;
    }

    public static void SetCustomRole(this PlayerControl pc, CustomRole role, CustomRole[]? subRoles = null)
    {
        if (!pc) return;
        if (role == null)
        {
            Main.Logger.LogError("It shouldn't be possible but the role to set is null. Try check if you're using a generic SetCustomRole<T> method and setting a role which is not registered yet.");
            return;
        }

        var playerRole = GameUtils.PlayerData.FirstOrDefault(pr => pr.Player.IsSamePlayer(pc));
        if (playerRole is not null) GameUtils.PlayerData.Remove(playerRole);

        GameUtils.PlayerData.Add(new PlayerData(pc.Data, role, subRoles));
        RoleManager.Instance.SetRole(pc, role.BaseRoleType);

        Main.Logger.LogInfo($"The role of player {pc.Data.PlayerName} has set to {role.GetNormalName()}");
    }

    public static void SetCustomRole<T>(this PlayerControl pc) where T : CustomRole
    {
        if (!pc) return;
        var role = CustomRoleManager.GetManager().GetTypeRoleInstance<T>();
        pc.SetCustomRole(role);
    }

    public static void RpcSetCustomRole(this PlayerControl pc, CustomRole role)
    {
        if (!pc) return;
        var writer = RpcUtils.StartRpcImmediately(pc, KnownRpc.SetRole);
        writer.Write(pc.PlayerId);
        writer.WritePacked(role.Id);
        writer.Finish();
        SetCustomRole(pc, role);
    }

    public static void RpcSetCustomRole<T>(this PlayerControl pc) where T : CustomRole
    {
        if (!pc) return;
        var role = CustomRoleManager.GetManager().GetTypeRoleInstance<T>();
        var writer = RpcUtils.StartRpcImmediately(pc, KnownRpc.SetRole);
        writer.Write(pc.PlayerId);
        writer.WritePacked(role.Id);
        writer.Finish();
        SetCustomRole(pc, role);
    }

    public static CustomRole[] GetSubRoles(this PlayerControl pc)
    {
        return pc.Data.GetSubRoles();
    }

    public static CustomRole[] GetSubRoles(this NetworkedPlayerInfo pc)
    {
        var data = pc.GetPlayerData();
        if (data == null)
            return Array.Empty<CustomRole>();
        return data.SubRoles;
    }

    public static void LocalDieWithReason(this PlayerControl pc, PlayerControl target, DeathReason reason,
        bool showCorpse = true)
    {
        _ = new DeadPlayer(DateTime.Now, reason, target.Data, pc.Data);
        if (showCorpse)
            pc.MurderPlayer(target, GameUtils.DefaultFlag);
        else
            pc.Exiled();
    }

    public static bool CanKill(this PlayerControl pc)
    {
        return pc.GetMainRole().CanKill;
    }

    public static void RpcMurderAndModifyKillAnimation(this PlayerControl pc, PlayerControl target, PlayerControl toShowAsKiller)
    {
        pc.MurderPlayer(target, SucceededFlags);
        RpcUtils.StartRpcImmediately(pc, KnownRpc.MurderAndModifyKillAnimation)
            .WriteNetObject(target).WriteNetObject(toShowAsKiller).Finish();
    }
}

public enum DeathReason
{
    Unknown = -1,
    Disconnected,
    Default,
    Exiled,
    LoverSuicide
}

[SuppressMessage("Performance", "CA1822:将成员标记为 static")]
public class DeadPlayerListener : IListener
{
    [EventHandler(EventHandlerType.Postfix)]
    private void OnMurderPlayer(PlayerMurderEvent @event)
    {
        var target = @event.Target;
        var killer = @event.Player;
        if (!(target.Data.IsDead && killer && target)) return;
        if (DeadPlayerManager.DeadPlayers.Any(p => p.PlayerId == target.PlayerId)) return;

        var reason = DeadPlayerManager.GetDeathReason(killer, target);
        _ = new DeadPlayer(DateTime.Now, reason, target.Data, killer.Data);
    }

    [EventHandler(EventHandlerType.Prefix)]
    private void OnPlayerLeft(AmongUsClientLeaveEvent @event)
    {
        var data = @event.ClientData;
        if (!GameStates.InGame) return;
        _ = new DeadPlayer(DateTime.Now, DeathReason.Disconnected, data.Character.Data, null);
    }

    [EventHandler(EventHandlerType.Postfix)]
    private void OnPlayerExile(PlayerExileEndEvent @event)
    {
        var exiled = @event.ExileController.initData.networkedPlayer;
        if (exiled == null || DeadPlayerManager.DeadPlayers.Any(p => p.PlayerId == exiled.PlayerId)) return;
        _ = new DeadPlayer(DateTime.Now, DeathReason.Exiled, exiled, null);
    }

    [EventHandler(EventHandlerType.Postfix)]
    private void OnAirshipPlayerExile(PlayerExileEndOnAirshipEvent @event)
    {
        OnPlayerExile(new PlayerExileEndEvent(@event.Player, @event.Controller));
    }
}

public class DeadPlayerManager
{
    public static List<DeadPlayer> DeadPlayers { get; } = new();


    internal static DeathReason GetDeathReason(PlayerControl killer, PlayerControl target)
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
}

public class DeadPlayer
{
    public DeadPlayer(DateTime deadTime, DeathReason? deathReason, NetworkedPlayerInfo playerInfo, NetworkedPlayerInfo? killer)
    {
        DeadTime = deadTime;
        DeathReason = deathReason;
        Data = playerInfo;
        Killer = killer;
        VictimRole = playerInfo.GetMainRole();
        KillerRole = killer?.GetMainRole();
        PlayerId = playerInfo.PlayerId;
        DeadPlayerManager.DeadPlayers.Add(this);
    }

    public DateTime DeadTime { get; private set; }
    public DeathReason? DeathReason { get; }
    public NetworkedPlayerInfo Data { get; }
    public NetworkedPlayerInfo? Killer { get; }
    public CustomRole? VictimRole { get; private set; }
    public CustomRole? KillerRole { get; private set; }
    public byte PlayerId { get; }
}

[Serializable]
public class SerializablePlayerData
{
    public byte PlayerId { get; }
    
    public int MainRoleId { get; }
    
    public int[] SubRoleIds { get; }
    
    private SerializablePlayerData(byte playerId, int mainRoleId, int[] subRoleIds)
    {
        PlayerId = playerId;
        MainRoleId = mainRoleId;
        SubRoleIds = subRoleIds;
    }

    public PlayerData AsPlayerData()
    {
        return new PlayerData(GameData.Instance.GetPlayerById(PlayerId),
            CustomRoleManager.GetManager().GetRoleById(MainRoleId)!,
            SubRoleIds.Select(id => CustomRoleManager.GetManager().GetRoleById(id)).ToArray()!);
    }
    
    public static SerializablePlayerData Of(PlayerData playerData)
    {
        return new SerializablePlayerData(playerData.PlayerId, playerData.MainRole.Id,
            playerData.SubRoles.Select(role => role.Id).ToArray());
    }
}

public class PlayerData
{
    public PlayerData(NetworkedPlayerInfo data, CustomRole role, CustomRole[]? subRoles = null)
    {
        Player = data.Object;
        Data = data;
        MainRole = role;
        PlayerName = data.PlayerName;
        PlayerId = data.PlayerId;
        ColorId = data.DefaultOutfit.ColorId;
        Tags = new List<string>();
        SubRoles = subRoles != null
            ? subRoles.Where(subRole => subRole.IsSubRole).ToArray()
            : Array.Empty<CustomRole>();
    }
    
    public PlayerControl Player { get; }
    public NetworkedPlayerInfo Data { get; }
    public bool IsDisconnected => Data.Disconnected;
    public CustomRole MainRole { get; }
    public string PlayerName { get; }
    public byte PlayerId { get; }
    public int ColorId { get; }
    public CustomRole[] SubRoles { get; }
    
    public List<string> Tags { get; }
}