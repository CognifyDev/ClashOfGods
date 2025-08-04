using AmongUs.Data;
using COG.Config.Impl;
using COG.Game.Events;
using COG.Listener;
using COG.Listener.Event.Impl.AuClient;
using COG.Listener.Event.Impl.Player;
using COG.Patch;
using COG.Role;
using COG.Role.Impl;
using COG.Rpc;
using COG.UI.Vanilla.KillButton;
using Il2CppInterop.Runtime;
using InnerNet;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using UnityEngine;

namespace COG.Utils;

public enum ColorType
{
    Unknown = -1,
    Light,
    Dark
}

public static class PlayerUtils
{
    public static PoolablePlayer PoolablePlayerPrefab
            => Resources.FindObjectsOfTypeAll(Il2CppType.Of<PoolablePlayer>()).First().Cast<PoolablePlayer>();

    public static IEnumerable<CustomPlayerData> AllImpostors => GetPlayersByCamp(CampType.Impostor);

    public static IEnumerable<CustomPlayerData> AllCrewmates => GetPlayersByCamp(CampType.Crewmate);

    public static IEnumerable<CustomPlayerData> AllNeutrals => GetPlayersByCamp(CampType.Neutral);

    public static IEnumerable<CustomPlayerData> GetPlayersByCamp(CampType camp)
        => GameUtils.PlayerData.Where(pair => pair.Player && pair.MainRole.CampType == camp);

    /// <summary>
    ///     获取距离目标玩家位置最近的玩家
    /// </summary>
    /// <param name="target">目标玩家</param>
    /// <param name="mustAlive">是否必须为活着的玩家</param>
    /// <param name="closestDistance">限制距离</param>
    /// <returns>最近位置的玩家</returns>
    public static PlayerControl? GetClosestPlayer(this PlayerControl target, bool mustAlive = true,
        float closestDistance = float.PositiveInfinity, bool includeImpostor = true)
    {
        var targetLocation = target.GetTruePosition();
        var players = mustAlive ? GetAllAlivePlayers() : GetAllPlayers();

        PlayerControl? closestPlayer = null;

        foreach (var player in players)
        {
            if (player.IsSamePlayer(target)) continue;
            if (!includeImpostor && player.GetMainRole().CampType == CampType.Impostor) continue;

            var playerLocation = player.GetTruePosition();
            var distance = Vector2.Distance(targetLocation, playerLocation);

            if (distance >= closestDistance) continue;
            closestDistance = distance;
            closestPlayer = player;
        }

        return closestPlayer;
    }

    public const MurderResultFlags SucceededFlags = MurderResultFlags.Succeeded | MurderResultFlags.DecisionByHost;

    /// <summary>
    /// 杀死一个玩家不留下鸡腿
    /// </summary>
    /// <param name="killer"></param>
    /// <param name="target"></param>
    /// <param name="showAnimationToEverybody"></param>
    public static void RpcKillWithoutDeadBody(this PlayerControl killer, PlayerControl target, bool showAnimationToEverybody = false, bool anonymousKiller = true)
    {
        KillWithoutDeadBody(killer, target, showAnimationToEverybody);

        var rpc = RpcUtils.StartRpcImmediately(PlayerControl.LocalPlayer, KnownRpc.KillWithoutDeadBody);
        rpc.WriteNetObject(killer);
        rpc.WriteNetObject(target);
        rpc.Write(showAnimationToEverybody);
        rpc.Write(anonymousKiller);
        rpc.Finish();
    }

    /// <summary>
    ///     Kill without dead body
    /// </summary>
    /// <param name="killer"></param>
    /// <param name="target"></param>
    /// <param name="showAnimationToEverybody"></param>
    public static void KillWithoutDeadBody(this PlayerControl killer, PlayerControl target, bool showAnimationToEverybody = false, bool anonymousKiller = true)
    {
        DeadPlayer.Create(DateTime.Now, CustomDeathReason.Default, target.Data, killer.Data);
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

        var displayedKiller = anonymousKiller ? target : killer;

        if (target.IsSamePlayer(PlayerControl.LocalPlayer))
            HudManager.Instance.KillOverlay.ShowKillAnimation(killer.Data, target.Data); // Always show the real killer to the victim
        else if (showAnimationToEverybody)
            HudManager.Instance.KillOverlay.ShowKillAnimation(displayedKiller.Data, target.Data);
    }

    [SuppressMessage("ReSharper", "UseCollectionExpression")]
    public static List<PlayerControl> GetAllPlayers()
    {
        return new List<PlayerControl>(PlayerControl.AllPlayerControls.ToArray());
    }

    public static List<PlayerControl> GetAllAlivePlayers()
    {
        return GetAllPlayers().ToArray().Where(player => player.IsAlive()).ToList();
    }

    public static bool IsSamePlayer(this NetworkedPlayerInfo? info, NetworkedPlayerInfo? target)
    {
        if (!(info && target)) return false;
        return info == target;
    }

    public static bool IsSamePlayer(this PlayerControl? player, PlayerControl? target)
    {
        if (!(player && target)) return false;
        return player!.Data.IsSamePlayer(target!.Data);
    }

    public static DeadBody? GetDeadBody(this PlayerControl target)
    {
        return DeadBodyUtils.GetDeadBodies().FirstOrDefault(db => db.ParentId == target.PlayerId);
    }

    [return: NotNullIfNotNull(nameof(player))]
    public static CustomPlayerData? GetPlayerData(this PlayerControl? player)
    {
        if (!player)
            return null;
        return player!.Data.GetPlayerData();
    }

    [return: NotNullIfNotNull(nameof(player))]
    public static CustomPlayerData? GetPlayerData(this NetworkedPlayerInfo? player)
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
        if (targetRole == null) return false;

        return targetRole.SubRoles.Concat(targetRole.MainRole.ToSingleElementArray()).Contains(role);
    }

    public static DeadBody? GetClosestBody(List<DeadBody>? unTargetAble = null)
    {
        DeadBody? result = null;

        var num = PlayerControl.LocalPlayer.MaxReportDistance;
        if (!ShipStatus.Instance) return null;
        var position = PlayerControl.LocalPlayer.GetTruePosition();

        foreach (var body in Object.FindObjectsOfType<DeadBody>()
                     .Where(b => b.gameObject.active && (unTargetAble?.Contains(b) ?? true)))
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

    public static string GetLanguageDeathReason(this CustomDeathReason? deathReason)
    {
        var handler = new LanguageConfig.TextHandler("game.survival-data");
        return deathReason switch
        {
            CustomDeathReason.Default => handler.GetString("default"),
            CustomDeathReason.Disconnected => handler.GetString("disconnected"),
            CustomDeathReason.Exiled => handler.GetString("exiled"),
            CustomDeathReason.Misfire => handler.GetString("misfire"),
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
        pc.cosmetics.SetOutline(true, new(color));
    }

    public static void ClearOutline(this PlayerControl pc)
    {
        pc.cosmetics.SetOutline(false, new());
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

    public static void SetCustomRole(this PlayerControl pc, CustomRole? role, CustomRole[]? subRoles = null)
    {
        if (!pc) return;
        if (role == null)
        {
            Main.Logger.LogError("It shouldn't be possible but the role to set is null. Try checking if you're using a generic SetCustomRole<T> method and setting a role which is not registered yet.");
            return;
        }

        var playerRole = GameUtils.PlayerData.FirstOrDefault(pr => pr.Player.IsSamePlayer(pc));
        if (playerRole is not null)
        {
            playerRole.MainRole.ClearRoleGameData();
            playerRole.SubRoles.Do(r => r.ClearRoleGameData());
            GameUtils.PlayerData.Remove(playerRole);
        }

        CustomRole.ClearKillButtonSettings();
        GameUtils.PlayerData.Add(new CustomPlayerData(pc.Data, role, subRoles));
        RoleManager.Instance.SetRole(pc, role.BaseRoleType);
        VanillaKillButtonPatch.Initialize();

        Main.Logger.LogInfo($"The role of player {pc.Data.PlayerName} has been set to {role.GetNormalName()}");
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

    public static void LocalDieWithReason(this PlayerControl pc, PlayerControl target, CustomDeathReason reason,
        bool showCorpse = true)
    {
        _ = DeadPlayer.Create(DateTime.Now, reason, target.Data, pc.Data);
        if (showCorpse)
            pc.MurderPlayer(target, GameUtils.DefaultFlag);
        else
            pc.Exiled();
    }

    public static bool CanKill(this PlayerControl pc)
    {
        return pc.GetMainRole().CanKill;
    }

    public static void RpcMurderAndModifyKillAnimation(this PlayerControl pc, PlayerControl target, PlayerControl toShowAsKiller, bool modifyDeathData)
    {
        if (modifyDeathData)
            DeadPlayer.Create(DateTime.Now, CustomDeathReason.Default, target.Data, toShowAsKiller.Data);
        pc.MurderPlayer(target, SucceededFlags);
        RpcUtils.StartRpcImmediately(pc, KnownRpc.MurderAndModifyKillAnimation)
            .WriteNetObject(target).WriteNetObject(toShowAsKiller).Write(modifyDeathData).Finish();
    }

    public static void DisplayPlayerInfoOnName(this PlayerControl player, bool onlyDisplayNameSuffix = false)
    {
        var playerRole = player.GetPlayerData();
        if (playerRole is null || playerRole.MainRole is null) return;

        var subRoles = playerRole.SubRoles;
        var mainRole = playerRole.MainRole;
        var nameText = player.cosmetics.nameText;

        var nameTextBuilder = new StringBuilder();
        var subRoleNameBuilder = new StringBuilder();

        if (!onlyDisplayNameSuffix)
        {
            if (!subRoles.SequenceEqual(Array.Empty<CustomRole>()))
                foreach (var role in subRoles)
                    subRoleNameBuilder.Append(' ').Append(role.GetColorName());

            nameTextBuilder.Append(mainRole.Name)
                .Append(subRoleNameBuilder)
                .Append('\n').Append(player.Data.PlayerName);
        }
        else
        {
            nameTextBuilder.Append(player.Data.PlayerName);
        }

        var adtnalTextBuilder = new StringBuilder();
        if (!onlyDisplayNameSuffix)
        {
            foreach (var (color, text) in subRoles.ToList()
                     .Select(r => (
                         r.Color,
                         r.HandleAdditionalPlayerName(player)
                     )))
                adtnalTextBuilder.Append(' ').Append(text.Color(color));
        }
        else
        {
            var data = PlayerControl.LocalPlayer.GetPlayerData()!;
            foreach (var role in CustomRoleManager.GetManager().GetRoles())
                adtnalTextBuilder.Append(role.HandleAdditionalPlayerName(player));
        }

        nameTextBuilder.Append(adtnalTextBuilder);

        nameText.text = nameTextBuilder.ToString();

        if (!onlyDisplayNameSuffix)
            nameText.text = nameText.text.Color(mainRole.Color);
    }

    /// <summary>
    /// 检查 <see cref="GetClosestPlayer(PlayerControl, bool, float)"/> 返回的玩家是否在游戏设置击杀距离内
    /// </summary>
    /// <param name="closestValidPlayer"></param>
    /// <param name="mustAlive"></param>
    /// <param name="closestDistance"></param>
    /// <returns></returns>
    public static bool CheckClosestTargetInKillDistance(this PlayerControl player, out PlayerControl? closestValidPlayer, bool mustAlive = true, float closestDistance = float.PositiveInfinity)
    {
        if ((closestValidPlayer = player.GetClosestPlayer(mustAlive, closestDistance)) == null) return false;

        var localLocation = player.GetTruePosition();
        var targetLocation = closestValidPlayer.GetTruePosition();
        var distance = Vector2.Distance(localLocation, targetLocation);
        var valid = GameUtils.GetGameOptions().KillDistance >= distance;

        if (!valid)
            closestValidPlayer = null;

        return valid;
    }

    public static void RpcSuicide(this PlayerControl player) => player.CmdCheckMurder(player);

    public static void ResetKillCooldown(this PlayerControl player)
    {
        if (!player.AmOwner) return;

        var cooldown = player.GetKillButtonSetting()?.CustomCooldown() ?? -1;
        player.SetKillTimer(cooldown < 0
                            ? GameManager.Instance.LogicOptions.GetKillCooldown()
                            : cooldown);
    }

    public static KillButtonSetting? GetKillButtonSetting(this PlayerControl player)
    {
        if (!player.AmOwner) return null;

        var killButton = HudManager.Instance.KillButton;

        var settings = GetRoles(player).Select(r => r.CurrentKillButtonSetting);

        killButton.ToggleVisible(settings.Any(r => r.ForceShow()) && VanillaKillButtonPatch.IsHudActive);

        var activatedSettings = settings.Where(s => s.ForceShow());
        if (activatedSettings.Count() == 0) return null;
        return activatedSettings.First(); // there should be one settings active
    }

    public static CustomRole[] GetRoles(this PlayerControl player) => player.GetSubRoles().Concat(player.GetMainRole().ToSingleElementArray()).ToArray();

    public static void RpcDie(this PlayerControl player, CustomDeathReason reason)
    {
        player.StartRpcImmediately(KnownRpc.DieWithoutAnimationAndBody).WritePacked((int)reason).Finish();
        player.DieWithoutAnimationAndBody(reason);
    }

    public static void DieWithoutAnimationAndBody(this PlayerControl player, CustomDeathReason reason)
    {
        DeadPlayer.Create(DateTime.Now, reason, player.Data, null);
        EventRecorder.Instance.RecordTypeEvent(GameEventType.Die, player.GetPlayerData(), reason);
        player.Exiled();
    }
}

public enum CustomDeathReason
{
    Unknown = -1,
    Disconnected,
    Default,
    Exiled,
    Misfire,
    InteractionAfterRevival
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
        if (DeadPlayer.IsMarkedAsDead(target)) return;

        DeadPlayer.Create(DateTime.Now, CustomDeathReason.Default, target.Data, killer.Data);
    }

    [EventHandler(EventHandlerType.Prefix)]
    private void OnPlayerLeft(AmongUsClientLeaveEvent @event)
    {
        var data = @event.ClientData;
        if (!GameStates.InRealGame) return;
        DeadPlayer.Create(DateTime.Now, CustomDeathReason.Disconnected, data.Character.Data, null);
    }

    [EventHandler(EventHandlerType.Postfix)]
    private void OnPlayerExile(PlayerExileEndEvent @event)
    {
        var exiled = @event.ExileController.initData.networkedPlayer;
        if (exiled == null || DeadPlayer.DeadPlayers.Any(p => p.PlayerId == exiled.PlayerId)) return;
        DeadPlayer.Create(DateTime.Now, CustomDeathReason.Exiled, exiled, null);
    }

    [EventHandler(EventHandlerType.Postfix)]
    private void OnAirshipPlayerExile(PlayerExileEndOnAirshipEvent @event)
    {
        OnPlayerExile(new PlayerExileEndEvent(@event.Player, @event.Controller));
    }
}

// Use EventRecorder to replace one day
public class DeadPlayer
{
    public static List<DeadPlayer> DeadPlayers { get; } = new();

    private DeadPlayer(DateTime deadTime, CustomDeathReason? deathReason, NetworkedPlayerInfo playerInfo, NetworkedPlayerInfo? killer, bool forceAdd)
    {
        DeadTime = deadTime;
        DeathReason = deathReason;
        Data = playerInfo;
        Killer = killer;
        VictimRole = playerInfo.GetMainRole();
        KillerRole = killer?.GetMainRole();
        PlayerId = playerInfo.PlayerId;

        if (!DeadPlayers.Any(d => d.PlayerId == PlayerId) || forceAdd)
            DeadPlayers.Add(this);
    }

    public static DeadPlayer Create(DateTime deadTime, CustomDeathReason? deathReason, NetworkedPlayerInfo playerInfo, NetworkedPlayerInfo? killer, bool forceAdd = false)
    {
        return new(deadTime, deathReason, playerInfo, killer, forceAdd);
    }

    public DateTime DeadTime { get; private set; }
    public CustomDeathReason? DeathReason { get; }
    public NetworkedPlayerInfo Data { get; }
    public NetworkedPlayerInfo? Killer { get; }
    public CustomRole? VictimRole { get; private set; }
    public CustomRole? KillerRole { get; private set; }
    public byte PlayerId { get; }

    public static bool IsMarkedAsDead(NetworkedPlayerInfo player) => DeadPlayers.Any(p => p.PlayerId == player.PlayerId);
    public static bool IsMarkedAsDead(PlayerControl player) => IsMarkedAsDead(player.Data);
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

    public CustomPlayerData AsPlayerData()
    {
        return new CustomPlayerData(GameData.Instance.GetPlayerById(PlayerId),
            CustomRoleManager.GetManager().GetRoleById(MainRoleId)!,
            SubRoleIds.Select(id => CustomRoleManager.GetManager().GetRoleById(id)).ToArray()!);
    }

    public static SerializablePlayerData Of(CustomPlayerData playerData)
    {
        return new SerializablePlayerData(playerData.PlayerId, playerData.MainRole.Id,
            playerData.SubRoles.Select(role => role.Id).ToArray());
    }
}

public class CustomPlayerData
{
    public CustomPlayerData(NetworkedPlayerInfo data, CustomRole role, CustomRole[]? subRoles = null)
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

    public override bool Equals(object? obj)
    {
        if (obj == null || obj is not CustomPlayerData data)
            return false;

        return data.PlayerId == PlayerId;
    }

    public override int GetHashCode()
    {
        return PlayerId;
    }

    public bool IsRole<T>() where T : CustomRole
    {
        return MainRole is T || SubRoles.Any(r => r is T);
    }
}