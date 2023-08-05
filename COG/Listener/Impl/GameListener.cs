using System.Collections.Generic;
using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Role;
using COG.Role.Impl;
using COG.Rpc;
using COG.Utils;
using InnerNet;
using UnityEngine;
using Action = System.Action;
using GameStates = COG.States.GameStates;

namespace COG.Listener.Impl;

public class GameListener : IListener
{
    private static readonly List<IListener> RoleListeners = new();
    // private static bool _forceStarted;

    public void OnCoBegin()
    {
        // _forceStarted = false;
        GameStates.InGame = true;
        Main.Logger.LogInfo("Game started!");

        foreach (var (key, value) in GameUtils.Data)
        {
            RoleManager.Instance.SetRole(key, value.BaseRoleType);
        }
    }

    public void OnRPCReceived(byte callId, MessageReader reader)
    {
        var knownRpc = (KnownRpc)callId;
        if (knownRpc != KnownRpc.ShareRoles) return;
        var roleData = reader.ReadPackedInt32();
        var data = new Dictionary<PlayerControl, Role.Role>();
        for (var i = 0; i < roleData; i++)
        {
            var playerId = reader.ReadByte();
            var role = Role.RoleManager.GetManager().GetRoleByClassName(reader.ReadString());
            if (role == null) continue;
            var player = PlayerUtils.GetPlayerById(playerId);
            if (player == null) continue;
            data.Add(player, role);
        }

        GameUtils.Data = data;
    }

    public void AfterPlayerFixedUpdate(PlayerControl player)
    {
        GameOptionsManager.Instance.currentNormalGameOptions.RoleOptions.SetRoleRate(RoleTypes.Scientist, 0, 0);
        GameOptionsManager.Instance.currentNormalGameOptions.RoleOptions.SetRoleRate(RoleTypes.Engineer, 0, 0);
        GameOptionsManager.Instance.currentNormalGameOptions.RoleOptions.SetRoleRate(RoleTypes.GuardianAngel, 0, 0);
        GameOptionsManager.Instance.currentNormalGameOptions.RoleOptions.SetRoleRate(RoleTypes.Shapeshifter, 0, 0);
    }

    public void OnSelectRoles()
    {
        GameUtils.Data.Clear(); // 首先清除 防止干扰

        if (!AmongUsClient.Instance.AmHost) return; // 不是房主停止分配

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
            
            GameUtils.Data.Add(player, role);
        }
        
        // 打印职业分配信息
        foreach (var (player, value) in GameUtils.Data)
        {
            Main.Logger.LogInfo($"{player.name}({player.Data.FriendCode}) => {value.GetType().Name}");
        }
        
        foreach (var (key, value) in GameUtils.Data)
        {
            RoleListeners.Add(value.GetListener(key));
        }
        
        ShareRoles();
    }

    private void ShareRoles()
    {
        var writer = RpcUtils.StartRpcImmediately(PlayerControl.LocalPlayer, (byte)KnownRpc.ShareRoles);
        writer.WritePacked(GameUtils.Data.Count);
        foreach (var (key, value) in GameUtils.Data)
        {
            writer.Write(key.PlayerId);
            writer.Write(value.GetType().Name);
        }
        writer.Finish();
    }

    public class RoleShare : InnerNetObject
    {
        public Dictionary<PlayerControl, Role.Role> Data { get; }
        
        public RoleShare(Dictionary<PlayerControl, Role.Role> data)
        {
            Data = data;
        }
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
        // 取消已经注册的Listener
        foreach (var roleListener in RoleListeners)
        {
            ListenerManager.GetManager().UnregisterListener(roleListener);
        }
        RoleListeners.Clear();
    }

    public bool OnCheckGameEnd()
    {
        var aliveImpostors = new List<PlayerControl>();
        foreach (var (key, value) in GameUtils.Data)
        {
            if (value.CampType == CampType.Impostor) aliveImpostors.Add(key);
        }

        if (PlayerUtils.GetAllAlivePlayers().Count <= 1)
        {
            TempData.winners.Clear();
            return true;
        }

        if (aliveImpostors.Count >= PlayerUtils.GetAllAlivePlayers().Count)
        {
            TempData.winners.Clear();
            foreach (var aliveImpostor in aliveImpostors)
            {
                TempData.winners.Add(new WinningPlayerData(aliveImpostor.Data));
            }
            return true;
        }

        return false; // 不允许游戏结束
    }

    public bool OnPlayerVent(Vent vent, GameData.PlayerInfo playerInfo, ref bool canUse, ref bool couldUse, ref float cooldown)
    {
        foreach (var (key, value) in GameUtils.Data)
        {
            if (!key.Data.IsSamePlayer(playerInfo)) continue;
            var ventAble = value.CanVent;
            canUse = ventAble;
            couldUse = ventAble;
            cooldown = float.MaxValue;
            return false;
        }

        return true;
    }

    public void OnKeyboardPass()
    {
        // 有BUG 暂时不用
        /*
        if (GameStates.InGame || !GameStates.IsLobby || GameStates.IsMeeting || GameStates.IsVoting || GameStates.IsInTask) return;
        if (GameStartManager.Instance.startState != GameStartManager.StartingStates.Countdown) return;
        if (_forceStarted) return;
        if ((!Input.GetKey(KeyCode.RightShift) && !Input.GetKey(KeyCode.LeftShift)) || _forceStarted) return;
        _forceStarted = true;
        GameStartManager.Instance.countDownTimer = 1f;
        */
    }

    public void OnHudUpdate(HudManager manager)
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null) return;
        Role.Role? role;
        try
        {
            role = player.GetRoleInstance();
        }
        catch
        {
            return;
        }
        if (role == null) return;

        if (role.CanKill)
        {
            manager.KillButton.ToggleVisible(true);
            player.Data.Role.CanUseKillButton = true;
            manager.KillButton.gameObject.SetActive(true);
        }
        else
        {
            manager.KillButton.SetDisabled();
            manager.KillButton.ToggleVisible(false);
            manager.KillButton.gameObject.SetActive(false);
        }

        if (role.CanVent)
        {
            manager.ImpostorVentButton.ToggleVisible(true);
            player.Data.Role.CanVent = true;
            manager.ImpostorVentButton.gameObject.SetActive(true);
        }
        else
        {
            manager.ImpostorVentButton.SetDisabled();
            manager.ImpostorVentButton.ToggleVisible(false);
            manager.ImpostorVentButton.gameObject.SetActive(false);
        }

        if (role.CanSabotage)
        {
            manager.SabotageButton.ToggleVisible(true);
            manager.SabotageButton.gameObject.SetActive(true);
        }
        else
        {
            manager.SabotageButton.SetDisabled();
            manager.SabotageButton.ToggleVisible(false);
            manager.SabotageButton.gameObject.SetActive(false);
        }
    }
}