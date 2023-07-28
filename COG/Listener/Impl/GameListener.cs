using System.Collections.Generic;
using COG.Config.Impl;
using COG.Role;
using COG.Role.Impl;
using COG.Utils;
using InnerNet;
using Reactor.Networking;
using GameStates = COG.States.GameStates;

namespace COG.Listener.Impl;

public class GameListener : IListener
{
    public void OnCoBegin()
    {
        GameStates.InGame = true;
    }

    public void OnSelectRoles()
    {
        if (!AmongUsClient.Instance.AmHost) return; // 不是房主停止分配

        GameUtils.Data.Clear(); // 首先清除 防止干扰

        // 开始分配职业
        var players = PlayerUtils.GetAllPlayers().ToList().Disarrange(); // 打乱玩家顺序

        var rolesToAdd = new List<Role.Role>(); // 新建集合，用来存储可用的职业 

        // 获取最大可以启用的内鬼数量
        var maxImpostorsNum = GameUtils.GetImpostorsNum();

        // 新建一个获取器
        var getter = Role.RoleManager.GetManager().NewGetter();

        // 获取已经获取的内鬼职业数量
        var equalsImpostorsNum = 0;

        // 开始获取可以分配的职业
        while (getter.HasNext())
        {
            var next = getter.GetNext();
            if (next == null) continue;
            if (rolesToAdd.Count >= players.Count) break;
            if (equalsImpostorsNum >= maxImpostorsNum && next.CampType == CampType.Impostor) continue;
            if (next.CampType == CampType.Impostor) equalsImpostorsNum++;
            rolesToAdd.Add(next);
        }

        for (var i = 0; i < players.Count; i++)
        {
            var player = players[i];
            Role.Role role;
            try
            {
                role = rolesToAdd[i];
            }
            catch
            {
                role = Role.RoleManager.GetManager().GetTypeRoleInstance<Unknown>()!;
            }

            RoleManager.Instance.SetRole(player, role.BaseRoleType);
            GameUtils.Data.Add(player, role);
        }
        ShareRoles(new RolesShare(GameUtils.Data));
    }

    private void ShareRoles(RolesShare rolesShare)
    {
        var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, 
            (byte)KnownRpc.ShareRoles, SendOption.Reliable);
        writer.WriteNetObject(rolesShare);
    }

    public class RolesShare : InnerNetObject
    {
        private readonly Dictionary<PlayerControl, Role.Role> _dictionary;
        
        public RolesShare(Dictionary<PlayerControl, Role.Role> dictionary)
        {
            _dictionary = dictionary;
        }

        public Dictionary<PlayerControl, Role.Role> GetRolesInformation()
        {
            return _dictionary;
        }
    }

    public void OnGameEnd(AmongUsClient client, EndGameResult endGameResult)
    {
        
    }

    public void OnGameStart(GameStartManager manager)
    {
        // 改变按钮颜色
        manager.MakePublicButton.color = Palette.DisabledClear;
        manager.privatePublicText.color = Palette.DisabledClear;
    }

    public bool OnMakePublic(GameStartManager manager)
    {
        if (!AmongUsClient.Instance.AmHost) return false;
        GameUtils.SendGameMessage(LanguageConfig.Instance.MakePublicMessage);
        // 禁止设置为公开
        return false;
    }

    public void OnSetUpRoleText(IntroCutscene intro)
    {
        
    }

    public void OnSetUpTeamText(IntroCutscene intro)
    {
        
    }

    public void OnGameEndSetEverythingUp(EndGameManager manager)
    {
        
    }
}