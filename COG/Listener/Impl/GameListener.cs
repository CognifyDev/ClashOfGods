using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using COG.Config.Impl;
using COG.Role;
using COG.Role.Impl;
using COG.Utils;
using InnerNet;
using TMPro;
using UnityEngine;
using Enumerable = System.Linq.Enumerable;
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
        
        // 打印职业分配信息
        foreach (var (player, value) in GameUtils.Data)
        {
            Main.Logger.LogInfo($"{player.name}({player.Data.FriendCode}) => {value.GetType().Name}");
        }
        
        ShareRoles(RolesShare.Create(GameUtils.Data));
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

        public static RolesShare Create(Dictionary<PlayerControl, Role.Role> dictionary)
        {
            return new RolesShare(dictionary);
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
        Main.Logger.LogInfo("Setup role text for players...");
        Task.Run(async delegate
        {
            await Task.Delay(TimeSpan.FromMilliseconds(0.99999999999999994638));
            
            var myRole = GameUtils.GetLocalPlayerRole();
            if (myRole == null)
            {
                return;
            }
            
            intro.YouAreText.color = myRole.Color;
            intro.RoleText.text = myRole.Name;
            intro.RoleText.color = myRole.Color;
            intro.RoleBlurbText.color = myRole.Color;
            intro.RoleBlurbText.text = myRole.Description;
        });
    }

    public void OnSetUpTeamText(IntroCutscene intro,
        ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
    {
        var role = GameUtils.GetLocalPlayerRole();
        var player = PlayerControl.LocalPlayer;

        if (role == null) return;
        var camp = role.CampType;
        if (camp is CampType.Neutral or CampType.Unknown)
        {
            intro.BackgroundBar.material.color = camp.GetColor();
            intro.TeamTitle.text = camp.GetName();
            intro.TeamTitle.color = camp.GetColor();
            intro.ImpostorTitle.text = camp.GetName();
            intro.ImpostorTitle.color = camp.GetColor();
            intro.ImpostorName.text = camp.GetName();
            intro.ImpostorName.color = camp.GetColor();
            intro.ImpostorText.text = camp == CampType.Neutral ? LanguageConfig.Instance.NeutralCampDescription : 
                LanguageConfig.Instance.UnknownCampDescription;
            var soloTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            soloTeam.Add(player);
            teamToDisplay = soloTeam;

            // debug
            var type1 = intro.GetType();
            var propertyInfos1 = type1.GetProperties();
            Main.Logger.LogInfo(propertyInfos1.Length);
            var i = 0;
            foreach (var propertyInfo in propertyInfos1)
            {
                i++;
                if (propertyInfo.PropertyType.FullName != null &&
                    propertyInfo.PropertyType.FullName.ToLower().Contains("TextMeshPro".ToLower()))
                {
                    var textMeshPro = (TextMeshPro) propertyInfo.GetValue(intro)!;
                    System.Console.WriteLine(propertyInfo.Name + " -> " + textMeshPro.text);
                    File.WriteAllText($"{i + ""}.txt", textMeshPro.text, Encoding.UTF8);
                }
            }
            return;
        }
        
        intro.BackgroundBar.material.color = camp.GetColor();
        intro.TeamTitle.text = camp.GetName();
        intro.TeamTitle.color = camp.GetColor();
        intro.ImpostorTitle.text = camp.GetName();
        intro.ImpostorTitle.color = camp.GetColor();
        intro.ImpostorName.text = camp.GetName();
        intro.ImpostorName.color = camp.GetColor();
        intro.ImpostorText.text = camp == CampType.Crewmate ? LanguageConfig.Instance.CrewmateCampDescription : 
            LanguageConfig.Instance.ImpostorCampDescription;
        
        // debug
        var type = intro.GetType();
        var propertyInfos = type.GetProperties();
        Main.Logger.LogInfo(propertyInfos.Length);
        var i1 = 0;
        foreach (var propertyInfo in propertyInfos)
        {
            i1++;
            if (propertyInfo.PropertyType.FullName != null &&
                propertyInfo.PropertyType.FullName.ToLower().Contains("TextMeshPro".ToLower()))
            {
                var textMeshPro = (TextMeshPro) propertyInfo.GetValue(intro)!;
                Main.Logger.LogInfo(propertyInfo.Name + " -> " + textMeshPro.text);
                File.WriteAllText($"{i1 + ""}.txt", textMeshPro.text, Encoding.UTF8);
            }
        }

        if (camp != CampType.Impostor) return;
        var team = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
        foreach (var keyValuePair in Enumerable.Where(GameUtils.Data, keyValuePair => keyValuePair.Value.CampType == CampType.Impostor))
        {
            team.Add(keyValuePair.Key);
        }

        teamToDisplay = team;
    }

    public void OnGameEndSetEverythingUp(EndGameManager manager)
    {
        
    }
}