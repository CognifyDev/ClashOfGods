using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using COG.Constant;
using COG.Listener.Event.Impl.RManager;
using COG.Role;
using COG.Rpc;
using COG.Utils;

namespace COG.Listener.Impl;

public class RoleAssignmentListener : IListener
{
    private readonly RpcHandler<int, CustomPlayerData[]> _roleSelectionShareRpcHandler;

    public RoleAssignmentListener()
    {
        _roleSelectionShareRpcHandler = new RpcHandler<int, CustomPlayerData[]>(KnownRpc.ShareRoles,
            onPerform: (_, playerData) =>
            {
                foreach (var data in playerData)
                {
                    GameUtils.PlayerData.Add(data);
                    data.Player.SetCustomRole(data.MainRole, data.SubRoles);
                }

                Main.Logger.LogDebug(playerData.Select(pr =>
                    $"{pr.Player.Data.PlayerName}({pr.Player.Data.FriendCode})" +
                    $" => {pr.MainRole.GetNormalName()}{pr.SubRoles.AsString()}"));
            },
            onSend: (writer, count, data) =>
            {
                writer.WritePacked(count);
                foreach (var pd in data)
                    writer.WriteBytesAndSize(SerializablePlayerData.Of(pd).SerializeToData());
                Main.Logger.LogInfo("Successfully sent role assignment data!");
            },
            onReceive: reader =>
            {
                // Clear existing data before applying the authoritative assignment from host
                GameUtils.PlayerData.Clear();
                Main.Logger.LogInfo("Received role assignment data, applying...");

                var count = reader.ReadPackedInt32();
                var list = new List<CustomPlayerData>(count);
                for (var i = 0; i < count; i++)
                    list.Add(((byte[])reader.ReadBytesAndSize())
                        .DeserializeToData<SerializablePlayerData>()
                        .ToPlayerData());

                return (count, list.ToArray());
            }
        );

        IRpcHandler.Register(_roleSelectionShareRpcHandler);
    }
    
    private void SelectRoles()
    {
        GameUtils.PlayerData.Clear();

        if (!AmongUsClient.Instance.AmHost) return;

        try
        {
            var players = PlayerUtils.GetAllPlayers().ToArray();
            var impostorNumber = GameUtils.GetImpostorsNumber();
            var neutralNumber = GameUtils.GetNeutralNumber();

            ValidateAssignmentParameters(impostorNumber, neutralNumber, players.Length);

            if (!ValidateRoleConfiguration(impostorNumber, neutralNumber))
            {
                ApplyFallbackRoles();
                return;
            }

            var playerGetter = new PlayerGetter(players);
            var mainRoleData = new Dictionary<PlayerControl, CustomRole>();
            var subRoleData = new Dictionary<PlayerControl, CustomRole[]>();

            AssignMainRoles(playerGetter, mainRoleData, impostorNumber, neutralNumber);
            AssignSubRoles(subRoleData, GlobalCustomOptionConstant.MaxSubRoleNumber.GetInt());
            ApplyRolesToPlayers(mainRoleData, subRoleData);
            LogRoleAssignment();

            Main.Logger.LogInfo("Successfully selected roles.");
        }
        catch (System.Exception ex)
        {
            Main.Logger.LogError($"职业分配失败: {ex.Message}");
            ApplyFallbackRoles();
        }
    }

    private static bool ValidateRoleConfiguration(int impostorNumber, int neutralNumber)
    {
        var roles = CustomRoleManager.GetManager().GetRoles();

        // Count total available slots (accounting for per-role quantity options)
        var availableImpostorSlots = roles
            .Where(r => r.CampType == CampType.Impostor && r.IsAvailable())
            .Sum(r => r.RoleNumberOption?.GetInt() ?? 0);

        var availableNeutralSlots = roles
            .Where(r => r.CampType == CampType.Neutral && r.IsAvailable())
            .Sum(r => r.RoleNumberOption?.GetInt() ?? 0);

        if (availableImpostorSlots < impostorNumber)
            Main.Logger.LogWarning(
                $"内鬼自定义职业槽位不足: 需要 {impostorNumber}，可用 {availableImpostorSlots}，不足部分将以基础内鬼补全");

        if (availableNeutralSlots < neutralNumber)
        {
            Main.Logger.LogWarning(
                $"中立职业数量不足: 需要 {neutralNumber}，但只有 {availableNeutralSlots} 个可用，将回落到基础职业分配");
            return false;
        }

        return true;
    }

    private static void ValidateAssignmentParameters(int impostorNumber, int neutralNumber, int playerCount)
    {
        if (playerCount == 0)
            throw new InvalidOperationException("没有玩家可分配职业");

        if (impostorNumber < 0 || neutralNumber < 0)
            throw new ArgumentException(
                $"职业数量不能为负数: 内鬼={impostorNumber}, 中立={neutralNumber}");

        if (impostorNumber + neutralNumber > playerCount)
            Main.Logger.LogWarning(
                $"职业数量配置可能异常: 内鬼 {impostorNumber} + 中立 {neutralNumber} > 玩家总数 {playerCount}");
    }

    private static void AssignMainRoles(
        PlayerGetter playerGetter,
        Dictionary<PlayerControl, CustomRole> mainRoleData,
        int impostorNumber,
        int neutralNumber)
    {
        var manager = CustomRoleManager.GetManager();

        // Getters with base-role fallbacks: when custom roles run out, GetNext() returns the default
        var impostorGetter = manager.NewGetter(
            role => role.CampType == CampType.Impostor,
            manager.GetTypeRoleInstance<Impostor>());

        var crewmateGetter = manager.NewGetter(
            role => role.CampType == CampType.Crewmate,
            manager.GetTypeRoleInstance<Crewmate>());

        // Neutral has no fallback — only assign when HasNext() is true
        var neutralGetter = manager.NewGetter(role => role.CampType == CampType.Neutral);

        // Do NOT guard on impostorGetter.HasNext(); GetNext() already returns base Impostor as fallback
        for (var i = 0; i < impostorNumber; i++)
        {
            if (!playerGetter.HasNext()) break;
            mainRoleData[playerGetter.GetNext()] = impostorGetter.GetNext();
        }

        for (var i = 0; i < neutralNumber; i++)
        {
            if (!playerGetter.HasNext() || !neutralGetter.HasNext()) break;
            mainRoleData[playerGetter.GetNext()] = neutralGetter.GetNext();
        }

        while (playerGetter.HasNext())
            mainRoleData[playerGetter.GetNext()] = crewmateGetter.GetNext();
    }

    private static void AssignSubRoles(Dictionary<PlayerControl, CustomRole[]> subRoleData, int maxSubRoleNumber)
    {
        if (maxSubRoleNumber <= 0) return;

        var subRoleGetter = CustomRoleManager.GetManager().NewGetter(role => role.IsSubRole);
        if (!subRoleGetter.HasNext()) return;

        var allPlayers = PlayerUtils.GetAllPlayers();
        if (allPlayers.Count == 0) return;

        var rng = new Random();

        // Track per-player accumulated sub-roles using mutable lists for cheap appending
        var playerSubRoles = allPlayers.ToDictionary(p => p, _ => new List<CustomRole>());

        while (subRoleGetter.HasNext())
        {
            var subRole = subRoleGetter.GetNext();

            // Eligible: has room AND doesn't already hold this exact role type
            var eligible = playerSubRoles
                .Where(kvp =>
                    kvp.Value.Count < maxSubRoleNumber &&
                    kvp.Value.All(r => r.GetType() != subRole.GetType()))
                .Select(kvp => kvp.Key)
                .ToList();

            // No player can accept this sub-role; stop assigning
            if (eligible.Count == 0) break;

            playerSubRoles[eligible[rng.Next(eligible.Count)]].Add(subRole);
        }

        // Flush non-empty lists into the output dictionary
        foreach (var (player, roles) in playerSubRoles)
            if (roles.Count > 0)
                subRoleData[player] = roles.ToArray();
    }

    private static void ApplyRolesToPlayers(
        Dictionary<PlayerControl, CustomRole> mainRoleData,
        Dictionary<PlayerControl, CustomRole[]> subRoleData)
    {
        foreach (var (player, mainRole) in mainRoleData)
        {
            subRoleData.TryGetValue(player, out var subRoles);
            player.SetCustomRole(mainRole, subRoles ?? []);
        }
    }

    private static void LogRoleAssignment()
    {
        var sb = new StringBuilder();
        foreach (var pd in GameUtils.PlayerData)
        {
            var subNames = pd.SubRoles.Select(r => r.GetNormalName()).AsString();
            sb.AppendLine($"{pd.Player.name}({pd.PlayerId}) => {pd.MainRole.GetNormalName()} | sub: {subNames}");
        }
        Main.Logger.LogDebug(sb.ToString());
    }

    private static void ApplyFallbackRoles()
    {
        GameUtils.PlayerData.Clear();

        var players = PlayerUtils.GetAllPlayers().Disarrange();
        var crewmateRole = CustomRoleManager.GetManager().GetTypeRoleInstance<Crewmate>();
        var impostorRole = CustomRoleManager.GetManager().GetTypeRoleInstance<Impostor>();
        var impostorCount = Math.Min(GameUtils.GetImpostorsNumber(), players.Count);

        for (var i = 0; i < players.Count; i++)
            players[i].SetCustomRole(i < impostorCount ? impostorRole : crewmateRole, []);
    }

    [EventHandler(EventHandlerType.Prefix)]
    public bool OnSelectRoles(RoleManagerSelectRolesEvent _)
    {
        Main.Logger.LogInfo("Select roles for players...");
        SelectRoles();

        if (!GameUtils.PlayerData.Any()) return true;

        var playerData = GameUtils.PlayerData.ToArray();

        // Sync vanilla role type to all clients. canBeUndone=true allows the intro animation to play.
        foreach (var pd in playerData)
            pd.Player.RpcSetRole(pd.MainRole.BaseRoleType, true);

        Main.Logger.LogInfo("Share custom role data with all clients...");
        _roleSelectionShareRpcHandler.Send(playerData.Length, playerData);

        // Notify every assigned role that the sharing phase is complete
        var allAssignedRoles = playerData
            .Select(pd => pd.MainRole)
            .Concat(playerData.SelectMany(pd => pd.SubRoles));

        foreach (var role in allAssignedRoles)
            role.AfterSharingRoles();

        return false;
    }

    private class PlayerGetter(PlayerControl[] targets) : IGetter<PlayerControl>
    {
        private readonly List<PlayerControl> _players = new List<PlayerControl>(targets).Disarrange();

        public PlayerControl GetNext()
        {
            var target = _players[0];
            _players.RemoveAt(0);
            return target;
        }

        public bool HasNext() => _players.Count > 0;
        public int Number() => _players.Count;
        public void PutBack(PlayerControl value) => _players.Add(value);
    }
}
