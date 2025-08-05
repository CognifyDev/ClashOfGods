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
                // ���ԭ�б���ֹ����
                GameUtils.PlayerData.Clear();
                // ��ʼ��������
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
     * ְҵ�����߼���
     *
     * ְҵ�����������
     * 1.����Ҫ��֤���п��õ�ְҵ����������ɣ�Ȼ����ȥ�����ְҵ
     * 2.Ӧ����֤�ڹ��ȱ�������ȫ������Ǵ�Ա��������Ӫ
     * 3.��ְҵ����ҪС�ģ�Ϊ���㷨�Ŀ���Ӧ���ڷ�������ְҵ�������һͬ���丱ְҵ
     *
     */
    private static void SelectRoles()
    {
        // ������� ��ֹ����
        GameUtils.PlayerData.Clear();

        // ���Ƿ���ֹͣ����
        if (!AmongUsClient.Instance.AmHost) return;

        // ��ȡ���е���Ҽ���
        var playerGetter = new PlayerGetter(PlayerUtils.GetAllPlayers().ToArray());

        // ��ӵ��ֵ�
        var mainRoleData = new Dictionary<PlayerControl, CustomRole>();
        var subRoleData = new Dictionary<PlayerControl, CustomRole[]>();

        // ��ȡ�����Ա�����ĸ�ְҵ����
        var maxSubRoleNumber = GlobalCustomOptionConstant.MaxSubRoleNumber.GetInt();

        // ��ȡ������ϷҪ������ڹ�����
        var impostorNumber = GameUtils.GetImpostorsNumber();

        // ��ȡ������ϷҪ�������������
        var neutralNumber = GameUtils.GetNeutralNumber();

        // ��ȡһ����ְҵ��ȡ��
        var subRoleGetter = CustomRoleManager.GetManager().NewGetter(role => role.IsSubRole);

        // ��ȡһ���ڹ��ȡ��
        var impostorGetter = CustomRoleManager.GetManager().NewGetter(role => role.CampType == CampType.Impostor,
            CustomRoleManager.GetManager().GetTypeRoleInstance<Impostor>());

        // ����һ������ְҵ��ȡ��
        // ʵ���� CustomRoleManager.GetManager().GetTypeRoleInstance<Jester>() �Ƕ����
        // ��Ϊ�� GameUtils#GetNeutralNumber �������Ѿ��ƶ��˳��ϴ��ڵ����������������������õ���������
        var neutralGetter = CustomRoleManager.GetManager().NewGetter(role => role.CampType == CampType.Neutral);

        var crewmateGetter = CustomRoleManager.GetManager().NewGetter(role => role.CampType == CampType.Crewmate,
            CustomRoleManager.GetManager().GetTypeRoleInstance<Crewmate>());

        // ���ȷ����ڹ�ְҵ
        for (var i = 0; i < impostorNumber; i++)
        {
            if (!playerGetter.HasNext()) break;

            // ��ΪGetter������Ĭ��ֵ������������Ƿ�����һ��
            var impostorRole = impostorGetter.GetNext();

            // �����һ�����Ի�ȡ���ģ���Ϊ�����ҵ���Ŀ�����Ի�ȡ������ô�ڹ����ĿҲ�������1����ˣ�����һ�����Ҳû�У���Ȼ��һ�����Ի�ȡ����
            // ����Ҳ�����һ��Ҳû�У����һ�����Ի�ȡ��
            var target = playerGetter.GetNext();

            // �������
            mainRoleData.Add(target, impostorRole);
        }

        // ��������������ְҵ
        for (var i = 0; i < neutralNumber; i++)
        {
            if (!neutralGetter.HasNext()) break;

            if (!playerGetter.HasNext()) break;

            // ͬ���Ѿ�������Ĭ��ֵ��������
            var neutralRole = neutralGetter.GetNext();

            // ��ȡ���ʵ��
            var target = playerGetter.GetNext();

            // �������
            mainRoleData.Add(target, neutralRole);
        }

        // �����ŷ��䴬Աְҵ
        while (playerGetter.HasNext())
        {
            // ��ȡʵ��
            var cremateRole = crewmateGetter.GetNext();

            // ��ȡ���ʵ��
            var target = playerGetter.GetNext();

            // û��Ҫ�Ƴ�������б��У���Ϊ���������ò���players������
            // players = players.Where(player => !player.IsSamePlayer(target)).ToList();

            // �������
            mainRoleData.Add(target, cremateRole);
        }

        // ������һ�¸�ְҵ
        /*
         * ��ְҵ�����㷨���£�
         * �����ȡ��ұļ�ʽ�ط��Ÿ�ְҵ
         */
        var allPlayers = PlayerUtils.GetAllPlayers().Disarrange();

        /*
         * ��ְҵ�ķ����е�����
         * ��ְҵ����������Ŀ�Լ���ְҵ��Ŀ����
         * ������ķ���Ƚ��鷳
         *
         * ����Ҫ��ȷ������ҵ��ж��������������£�
         * ��������Ŀ = ��������Ŀ * �����Ŀ
         * ���ҽ��� ��ְҵ�ѷ�����Ŀ ���� ��ְҵӦ��������Ŀ(=��ְҵ������Ŀ > ��������Ŀ ? ��������Ŀ : ��ְҵ������Ŀ)
         * �������
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


        // ȫ����������ɣ�������Ӧ��һ��
        for (var i = 0; i < mainRoleData.Count; i++)
        {
            var target = mainRoleData.Keys.ToArray()[i];

            // �輧������û����дEquals���������ֻ������
            var subRolesList = subRoleData.Where(pair =>
                pair.Key.IsSamePlayer(target)).ToImmutableDictionary().Values.ToList();

            var subRoles = subRolesList.Any() ? subRolesList[0] : Array.Empty<CustomRole>();

            target.SetCustomRole(mainRoleData.Values.ToArray()[i], subRoles); // �ȱ�������ְҵ������ShareRole���ְҵ����ȥ��
        }

        // ��ӡְҵ������Ϣ
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