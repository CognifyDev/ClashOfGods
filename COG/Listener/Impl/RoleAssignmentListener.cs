using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    private readonly Random _random;

    public RoleAssignmentListener()
    {
        _random = new Random();
        _roleSelectionShareRpcHandler = new RpcHandler<int, CustomPlayerData[]>(KnownRpc.ShareRoles,
            (_, playerData) =>
            {
                foreach (var data in playerData)
                {
                    GameUtils.PlayerData.Add(data);
                    data.Player.SetCustomRole(data.MainRole, data.SubRoles);
                }

                Main.Logger.LogDebug(playerData.Select(playerRole =>
                    $"{playerRole.Player.Data.PlayerName}({playerRole.Player.Data.FriendCode})" +
                    $" => {playerRole.MainRole.GetNormalName()}{playerRole.SubRoles.AsString()}"));
            },
            (writer, count, data) =>
            {
                writer.WritePacked(count);

                foreach (var playerData in data)
                    writer.WriteBytesAndSize(SerializablePlayerData.Of(playerData).SerializeToData());

                Main.Logger.LogInfo("Successfully sent role assignment data!");
            },
            reader =>
            {
                // 清除原列表，防止干扰
                GameUtils.PlayerData.Clear();
                // 开始读入数据
                Main.Logger.LogInfo("Received role assignment data, applying...");

                var count = reader.ReadPackedInt32();
                var list = new List<CustomPlayerData>();

                for (var i = 0; i < count; i++)
                    list.Add(((byte[])reader.ReadBytesAndSize()).DeserializeToData<SerializablePlayerData>()
                        .ToPlayerData());

                return (count, list.ToArray());
            }
        );

        IRpcHandler.Register(_roleSelectionShareRpcHandler);
    }
    /*
     * 职业分配逻辑：
     *
     * 职业分配基本条件
     * 1.首先要保证所有可用的职业都被分配完成，然后再去分配基职业
     * 2.应当保证内鬼先被分配完全，其次是船员和中立阵营
     * 3.副职业分配要小心，为了算法的快速应当在分配上述职业的情况下一同分配副职业
     */
    private void SelectRoles()
    {
        GameUtils.PlayerData.Clear();

        if (!AmongUsClient.Instance.AmHost) return;

        try
        {
            if (!ValidateRoleConfiguration())
            {
                ApplyFallbackRoles();
                return;
            }

            var players = PlayerUtils.GetAllPlayers().ToArray();
            var playerGetter = new PlayerGetter(players);

            ValidateAssignmentParameters(GameUtils.GetImpostorsNumber(), GameUtils.GetNeutralNumber(), players.Length);

            var mainRoleData = new Dictionary<PlayerControl, CustomRole>();
            var subRoleData = new Dictionary<PlayerControl, CustomRole[]>();

            AssignMainRoles(playerGetter, mainRoleData);
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

    private static bool ValidateRoleConfiguration()
    {
        try
        {
            var impostorRoles = CustomRoleManager.GetManager()
                .NewGetter(role => role.CampType == CampType.Impostor);
            var neutralRoles = CustomRoleManager.GetManager()
                .NewGetter(role => role.CampType == CampType.Neutral);

            var impostorNumber = GameUtils.GetImpostorsNumber();
            var neutralNumber = GameUtils.GetNeutralNumber();

            if (impostorRoles.Number() < impostorNumber)
            {
                Main.Logger.LogWarning($"内鬼职业数量不足: 需要{impostorNumber}个，但只有{impostorRoles.Number()}个可用");
                return false;
            }

            if (neutralRoles.Number() < neutralNumber)
            {
                Main.Logger.LogWarning($"中立职业数量不足: 需要{neutralNumber}个，但只有{neutralRoles.Number()}个可用");
                return false;
            }

            return true;
        }
        catch (System.Exception ex)
        {
            Main.Logger.LogError($"职业配置验证异常: {ex.Message}");
            return false;
        }
    }

    private static void ValidateAssignmentParameters(int impostorNumber, int neutralNumber, int playerCount)
    {
        if (impostorNumber < 0 || neutralNumber < 0)
        {
            throw new ArgumentException($"职业数量不能为负数: 内鬼={impostorNumber}, 中立={neutralNumber}");
        }

        if (impostorNumber + neutralNumber > playerCount)
        {
            Main.Logger.LogWarning($"职业数量配置可能异常: 内鬼{impostorNumber} + 中立{neutralNumber} > 玩家总数{playerCount}");
        }

        if (playerCount == 0)
        {
            throw new InvalidOperationException("没有玩家可分配职业");
        }
    }

    private static void AssignMainRoles(PlayerGetter playerGetter, Dictionary<PlayerControl, CustomRole> mainRoleData)
    {
        var impostorNumber = GameUtils.GetImpostorsNumber();
        var neutralNumber = GameUtils.GetNeutralNumber();

        var impostorGetter = CustomRoleManager.GetManager().NewGetter(
            role => role.CampType == CampType.Impostor,
            CustomRoleManager.GetManager().GetTypeRoleInstance<Impostor>());

        var neutralGetter = CustomRoleManager.GetManager().NewGetter(
            role => role.CampType == CampType.Neutral);

        var crewmateGetter = CustomRoleManager.GetManager().NewGetter(
            role => role.CampType == CampType.Crewmate,
            CustomRoleManager.GetManager().GetTypeRoleInstance<Crewmate>());

        for (var i = 0; i < impostorNumber; i++)
        {
            if (!playerGetter.HasNext()) break;
            if (!impostorGetter.HasNext()) break;

            var impostorRole = impostorGetter.GetNext();
            var target = playerGetter.GetNext();
            mainRoleData[target] = impostorRole;
        }

        for (var i = 0; i < neutralNumber; i++)
        {
            if (!neutralGetter.HasNext()) break;
            if (!playerGetter.HasNext()) break;

            var neutralRole = neutralGetter.GetNext();
            var target = playerGetter.GetNext();
            mainRoleData[target] = neutralRole;
        }

        while (playerGetter.HasNext())
        {
            var cremateRole = crewmateGetter.GetNext();
            var target = playerGetter.GetNext();
            mainRoleData[target] = cremateRole;
        }
    }

    private void AssignSubRoles(Dictionary<PlayerControl, CustomRole[]> subRoleData, int maxSubRoleNumber)
    {
        var subRoleGetter = CustomRoleManager.GetManager().NewGetter(role => role.IsSubRole);

        var subRoleEnabledNumber = subRoleGetter.Number();
        var allPlayers = PlayerUtils.GetAllPlayers();
        var subRoleMaxCanBeArrange = maxSubRoleNumber * allPlayers.Count;
        var subRoleShouldBeGivenNumber = subRoleEnabledNumber > subRoleMaxCanBeArrange
            ? subRoleMaxCanBeArrange
            : subRoleEnabledNumber;

        if (subRoleShouldBeGivenNumber <= 0) return;

        var availablePlayers = allPlayers
            .Where(p => !subRoleData.ContainsKey(p) || subRoleData[p].Length < maxSubRoleNumber)
            .Disarrange()
            .ToList();

        var givenTimes = 0;
        var maxAttempts = subRoleShouldBeGivenNumber * 20;

        while (givenTimes < subRoleShouldBeGivenNumber && availablePlayers.Count > 0 && maxAttempts-- > 0)
        {
            if (!subRoleGetter.HasNext()) break;

            var randomPlayer = availablePlayers[_random.Next(availablePlayers.Count)];
            subRoleData.TryGetValue(randomPlayer, out var existingRoles);

            if (existingRoles != null && existingRoles.Length >= maxSubRoleNumber)
            {
                availablePlayers.Remove(randomPlayer);
                continue;
            }

            var subRole = subRoleGetter.GetNext();
            var rolesList = existingRoles != null ? new List<CustomRole>(existingRoles) : new List<CustomRole>();

            if (rolesList.Any(role => role.GetType() == subRole.GetType()))
            {
                subRoleGetter.PutBack(subRole);
                continue;
            }

            rolesList.Add(subRole);
            subRoleData[randomPlayer] = rolesList.ToArray();
            givenTimes++;

            if (rolesList.Count >= maxSubRoleNumber)
            {
                availablePlayers.Remove(randomPlayer);
            }
        }
    }

    private static void ApplyRolesToPlayers(Dictionary<PlayerControl, CustomRole> mainRoleData,
        Dictionary<PlayerControl, CustomRole[]> subRoleData)
    {
        for (var i = 0; i < mainRoleData.Count; i++)
        {
            var target = mainRoleData.Keys.ToArray()[i];

            var subRolesList = subRoleData.Where(pair =>
                pair.Key.IsSamePlayer(target)).ToImmutableDictionary().Values.ToList();

            var subRoles = subRolesList.Any() ? subRolesList[0] : [];

            target.SetCustomRole(mainRoleData.Values.ToArray()[i], subRoles);
        }
    }

    private static void LogRoleAssignment()
    {
        var sb = new StringBuilder();
        foreach (var playerRole in GameUtils.PlayerData)
        {
            playerRole.Player.RpcSetRole(playerRole.MainRole.BaseRoleType);
            sb.AppendLine($"""
                           {playerRole.Player.name}({playerRole.PlayerId}) 
                               => {playerRole.MainRole.GetNormalName()}  with sub role(s):
                               {playerRole.SubRoles.Select(subRole => subRole.GetNormalName()).AsString()}
                           """);
        }

        Main.Logger.LogDebug(sb.ToString());
    }

    private static void ApplyFallbackRoles()
    {
        GameUtils.PlayerData.Clear();

        var players = PlayerUtils.GetAllPlayers();
        var crewmateRole = CustomRoleManager.GetManager().GetTypeRoleInstance<Crewmate>();
        var impostorRole = CustomRoleManager.GetManager().GetTypeRoleInstance<Impostor>();

        var impostorNumber = Math.Min(GameUtils.GetImpostorsNumber(), players.Count);
        var playerList = players.Disarrange().ToList();

        for (var i = 0; i < impostorNumber; i++)
        {
            var player = playerList[i];
            player.SetCustomRole(impostorRole, Array.Empty<CustomRole>());
        }

        for (var i = impostorNumber; i < playerList.Count; i++)
        {
            var player = playerList[i];
            player.SetCustomRole(crewmateRole, Array.Empty<CustomRole>());
        }
    }

    [EventHandler(EventHandlerType.Prefix)]
    public bool OnSelectRoles(RoleManagerSelectRolesEvent _)
    {
        Main.Logger.LogInfo("Select roles for players...");
        SelectRoles();

        if (!GameUtils.PlayerData.Any())
        {
            return true;
        }

        Main.Logger.LogInfo("Share roles for players...");
        var playerData = GameUtils.PlayerData.ToArray();

        foreach (var customPlayerData in playerData)
        {
            customPlayerData.Player.RpcSetRole(customPlayerData.MainRole.BaseRoleType, true);
        }

        _roleSelectionShareRpcHandler.Send(playerData.Length, playerData);

        var roleList = GameUtils.PlayerData.Select(pr => pr.MainRole).ToList();
        roleList.AddRange(GameUtils.PlayerData.SelectMany(pr => pr.SubRoles));

        foreach (var availableRole in roleList)
            availableRole.AfterSharingRoles();

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

        public bool HasNext()
        {
            return _players.Count > 0;
        }

        public int Number()
        {
            return _players.Count;
        }

        public void PutBack(PlayerControl value)
        {
            _players.Add(value);
        }
    }
}