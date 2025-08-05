using COG.Constant;
using COG.Listener.Event.Impl.RManager;
using COG.Role;
using COG.Rpc;
using COG.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace COG.Listener.Impl;

public class RoleAssignmentListener : IListener
{
    private readonly RpcHandler<int, CustomPlayerData[]> _roleSelectionShareRpcHandler;

    public RoleAssignmentListener()
    {
        _roleSelectionShareRpcHandler = new(KnownRpc.ShareRoles,
            (_, playerData) =>
            {
                foreach (var data in playerData)
                {
                    GameUtils.PlayerData.Add(data);
                    data.Player.SetCustomRole(data.MainRole, data.SubRoles);
                }

                Main.Logger.LogDebug(playerData.Select(playerRole => $"{playerRole.Player.Data.PlayerName}({playerRole.Player.Data.FriendCode})" +
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

                for (int i = 0; i < count; i++)
                    list.Add(((byte[])reader.ReadBytesAndSize()).DeserializeToData<SerializablePlayerData>().ToPlayerData());

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
     *
     */
    private static void SelectRoles()
    {
        // 首先清除 防止干扰
        GameUtils.PlayerData.Clear();

        // 不是房主停止分配
        if (!AmongUsClient.Instance.AmHost) return;

        // 获取所有的玩家集合
        var playerGetter = new PlayerGetter(PlayerUtils.GetAllPlayers().ToArray());

        // 添加到字典
        var mainRoleData = new Dictionary<PlayerControl, CustomRole>();
        var subRoleData = new Dictionary<PlayerControl, CustomRole[]>();

        // 获取最多可以被赋予的副职业数量
        var maxSubRoleNumber = GlobalCustomOptionConstant.MaxSubRoleNumber.GetInt();

        // 获取本局游戏要分配的内鬼数量
        var impostorNumber = GameUtils.GetImpostorsNumber();

        // 获取本局游戏要分配的中立数量
        var neutralNumber = GameUtils.GetNeutralNumber();

        // 获取一个副职业获取器
        var subRoleGetter = CustomRoleManager.GetManager().NewGetter(role => role.IsSubRole);

        // 获取一个内鬼获取器
        var impostorGetter = CustomRoleManager.GetManager().NewGetter(role => role.CampType == CampType.Impostor,
            CustomRoleManager.GetManager().GetTypeRoleInstance<Impostor>());

        // 创建一个中立职业获取器
        // 实际上 CustomRoleManager.GetManager().GetTypeRoleInstance<Jester>() 是多余的
        // 因为在 GameUtils#GetNeutralNumber 中我们已经制定了场上存在的中立数量是设置里面设置的中立数量
        var neutralGetter = CustomRoleManager.GetManager().NewGetter(role => role.CampType == CampType.Neutral);

        var crewmateGetter = CustomRoleManager.GetManager().NewGetter(role => role.CampType == CampType.Crewmate,
            CustomRoleManager.GetManager().GetTypeRoleInstance<Crewmate>());

        // 首先分配内鬼职业
        for (var i = 0; i < impostorNumber; i++)
        {
            if (!playerGetter.HasNext()) break;

            // 因为Getter设置了默认值，因此无需检测是否含有下一个
            var impostorRole = impostorGetter.GetNext();

            // 玩家是一定可以获取到的，因为如果玩家的数目不足以获取到，那么内鬼的数目也不会大于1，因此，除非一个玩家也没有，不然是一定可以获取到的
            // 而玩家不可能一个也没有，因此一定可以获取到
            var target = playerGetter.GetNext();

            // 添加数据
            mainRoleData.Add(target, impostorRole);
        }

        // 接下来分配中立职业
        for (var i = 0; i < neutralNumber; i++)
        {
            if (!neutralGetter.HasNext()) break;

            if (!playerGetter.HasNext()) break;

            // 同理，已经设置了默认值，无需检测
            var neutralRole = neutralGetter.GetNext();

            // 获取玩家实例
            var target = playerGetter.GetNext();

            // 添加数据
            mainRoleData.Add(target, neutralRole);
        }

        // 紧接着分配船员职业
        while (playerGetter.HasNext())
        {
            // 获取实例
            var cremateRole = crewmateGetter.GetNext();

            // 获取玩家实例
            var target = playerGetter.GetNext();

            // 没必要移除玩家在列表中，因为后面我们用不到players集合了
            // players = players.Where(player => !player.IsSamePlayer(target)).ToList();

            // 添加数据
            mainRoleData.Add(target, cremateRole);
        }

        // 最后分配一下副职业
        /*
         * 副职业分配算法如下：
         * 随机获取玩家蹦极式地发放副职业
         */
        var allPlayers = PlayerUtils.GetAllPlayers().Disarrange();

        /*
         * 副职业的分配有点特殊
         * 副职业有最大分配数目以及副职业数目限制
         * 因此它的分配比较麻烦
         *
         * 首先要明确分配玩家的判定条件，条件如下：
         * 最大分配数目 = 最大分配数目 * 玩家数目
         * 当且仅当 副职业已分配数目 等于 副职业应当分配数目(=副职业启用数目 > 最大分配数目 ? 最大分配数目 : 副职业启用数目)
         * 分配完成
         */
        var subRoleEnabledNumber = subRoleGetter.Number();
        var subRoleMaxCanBeArrange = maxSubRoleNumber * allPlayers.Count;
        var subRoleShouldBeGivenNumber = subRoleEnabledNumber > subRoleMaxCanBeArrange
            ? subRoleMaxCanBeArrange
            : subRoleEnabledNumber;

        var givenTimes = 0;

        while (givenTimes < subRoleShouldBeGivenNumber)
        {
            var random = new Random();
            var randomPlayer = allPlayers[random.Next(0, allPlayers.Count)];
            subRoleData.TryGetValue(randomPlayer, out var existRoles);
            if (existRoles != null && existRoles.Length >= maxSubRoleNumber) continue;
            var roles = new List<CustomRole>();
            if (existRoles != null)
            {
                roles.AddRange(existRoles);
            }

            var customRole = subRoleGetter.GetNext();

            if (roles.Contains(customRole))
            {
                subRoleGetter.PutBack(customRole);
                continue;
            }

            roles.Add(customRole);

            subRoleData.Add(randomPlayer, roles.ToArray());
            givenTimes++;
        }


        // 全部都分配完成，接下来应用一下
        for (var i = 0; i < mainRoleData.Count; i++)
        {
            var target = mainRoleData.Keys.ToArray()[i];

            // 歌姬树懒并没有重写Equals方法，因此只能这样
            var subRolesList = subRoleData.Where(pair =>
                pair.Key.IsSamePlayer(target)).ToImmutableDictionary().Values.ToList();

            var subRoles = subRolesList.Any() ? subRolesList[0] : Array.Empty<CustomRole>();

            target.SetCustomRole(mainRoleData.Values.ToArray()[i], subRoles); // 先本地设置职业，后面ShareRole会把职业发出去的
        }

        // 打印职业分配信息
        var sb = new StringBuilder();
        foreach (var playerRole in GameUtils.PlayerData)
            sb.AppendLine($"""
                {playerRole.Player.name}({playerRole.PlayerId}) 
                    => {playerRole.MainRole.GetNormalName()}  with sub role(s):
                    {playerRole.SubRoles.Select(subRole => subRole.GetNormalName()).AsString()}
                """);

        Main.Logger.LogWarning("Message below is for debugging, not for cheating!");
        Main.Logger.LogInfo(sb.ToString());
    }


    [EventHandler(EventHandlerType.Postfix)]
    public void OnSelectRoles(RoleManagerSelectRolesEvent @event)
    {
        Main.Logger.LogInfo("Select roles for players...");
        SelectRoles();

        Main.Logger.LogInfo("Share roles for players...");
        var playerData = GameUtils.PlayerData.ToArray();
        _roleSelectionShareRpcHandler.Send(playerData.Length, playerData);

        var roleList = GameUtils.PlayerData.Select(pr => pr.MainRole).ToList();
        roleList.AddRange(GameUtils.PlayerData.SelectMany(pr => pr.SubRoles));

        foreach (var availableRole in roleList) availableRole.AfterSharingRoles();
    }

    public class PlayerGetter : IGetter<PlayerControl>
    {
        public readonly List<PlayerControl> _players;

        public PlayerGetter(PlayerControl[] targets)
        {
            _players = new List<PlayerControl>(targets).Disarrange();
        }

        public PlayerControl GetNext()
        {
            var target = _players[0];
            _players.RemoveAt(0);

            return target;
        }

        public bool HasNext()
        {
            return !_players.IsEmpty();
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