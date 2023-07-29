using System.Collections.Generic;
using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Role;
using COG.Role.Impl;
using COG.Utils;
using InnerNet;
using UnityEngine;
using Action = System.Action;
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

    public bool OnSetUpRoleText(IntroCutscene intro, ref Il2CppSystem.Collections.IEnumerator roles)
    {
        Main.Logger.LogInfo("Setup role text for players...");

        var myRole = GameUtils.GetLocalPlayerRole();
        if (myRole == null)
        {
            return true;
        }

        var list = new List<Il2CppSystem.Collections.IEnumerator>();

        void SetupRoles()
        {
            if (GameOptionsManager.Instance.currentGameMode == GameModes.Normal)
            {
                intro.RoleText.text = myRole.Name;
                intro.RoleText.color = myRole.Color;
                intro.RoleBlurbText.text = myRole.Description;
                intro.RoleBlurbText.color = myRole.Color;
                intro.YouAreText.color = myRole.Color;
                
                intro.YouAreText.gameObject.SetActive(true);
                intro.RoleText.gameObject.SetActive(true);
                intro.RoleBlurbText.gameObject.SetActive(true);
                
                SoundManager.Instance.PlaySound(PlayerControl.LocalPlayer.Data.Role.IntroSound, false);
                
                if (intro.ourCrewmate == null)
                {
                    intro.ourCrewmate = intro.CreatePlayer(0, 1, PlayerControl.LocalPlayer.Data, false);
                    intro.ourCrewmate.gameObject.SetActive(false);
                }
                
                intro.ourCrewmate.gameObject.SetActive(true);
                var transform = intro.ourCrewmate.transform;
                transform.localPosition = new Vector3(0f, -1.05f, -18f);
                transform.localScale = new Vector3(1f, 1f, 1f);
                intro.ourCrewmate.ToggleName(false);
            }
        }

        list.Add(Effects.Action((Il2CppSystem.Action) (Action?)SetupRoles));
        list.Add(Effects.Wait(2.5f));
        void Action()
        {
            intro.YouAreText.gameObject.SetActive(false);
            intro.RoleText.gameObject.SetActive(false); 
            intro.RoleBlurbText.gameObject.SetActive(false);
            intro.ourCrewmate.gameObject.SetActive(false);
        }
        list.Add(Effects.Action((Il2CppSystem.Action) (Action?)Action));

        roles = Effects.Sequence(list.ToArray());
        return false;
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
            var soloTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            soloTeam.Add(player);
            teamToDisplay = soloTeam;
        }
    }

    public void AfterSetUpTeamText(IntroCutscene intro)
    {
        var role = GameUtils.GetLocalPlayerRole();

        if (role == null) return;
        var camp = role.CampType;
        
        intro.BackgroundBar.material.color = camp.GetColor();
        intro.TeamTitle.text = camp.GetName();
        intro.TeamTitle.color = camp.GetColor();
        
        intro.ImpostorText.text = "";
    }

    public void OnGameEndSetEverythingUp(EndGameManager manager)
    {
        
    }
}