using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Constant;
using COG.Game.CustomWinner;
using COG.Listener.Event.Impl.Game;
using COG.Listener.Event.Impl.GSManager;
using COG.Listener.Event.Impl.HManager;
using COG.Listener.Event.Impl.ICutscene;
using COG.Listener.Event.Impl.Player;
using COG.Listener.Event.Impl.RManager;
using COG.Listener.Event.Impl.VentImpl;
using COG.Role;
using COG.Role.Impl.Crewmate;
using COG.Rpc;
using COG.States;
using COG.UI.CustomExileText;
using COG.Utils;
using Il2CppSystem;
using Il2CppSystem.Collections;
using UnityEngine;
using Convert = System.Convert;
using Object = UnityEngine.Object;

namespace COG.Listener.Impl;

public class GameListener : IListener
{
    private static bool HasStartedRoom { get; set; }

    [EventHandler(EventHandlerType.Prefix)]
    public void OnCoBegin(IntroCutsceneCoBeginEvent @event)
    {
        GameStates.InGame = true;
        Main.Logger.LogInfo("Game started!");

        if (!AmongUsClient.Instance.AmHost) return;

        Main.Logger.LogInfo("Select roles for players...");
        SelectRoles();
        Main.Logger.LogInfo("Share roles for players...");
        ShareRoles();
    }

    [EventHandler(EventHandlerType.Prefix)]
    public void OnRPCReceived(PlayerHandleRpcEvent @event)
    {
        var callId = @event.CallId;
        var reader = @event.MessageReader;
        if (AmongUsClient.Instance.AmHost) return; // 是房主就返回
        var knownRpc = (KnownRpc)callId;

        switch (knownRpc)
        {
            case KnownRpc.ShareRoles:
                // 清除原列表，防止干扰
                GameUtils.PlayerRoleData.Clear();
                // 开始读入数据
                Main.Logger.LogInfo("The role data from the host was received by us.");

                var originalText = reader.ReadString()!;
                foreach (var s in originalText.Split(","))
                {
                    var texts = s.Split("|");
                    var player = PlayerUtils.GetPlayerById(Convert.ToByte(texts[0]));
                    var role = Role.RoleManager.GetManager().GetRoleById(Convert.ToInt32(texts[1]));
                    player!.SetCustomRole(role!);
                }

                foreach (var playerRole in GameUtils.PlayerRoleData)
                    Main.Logger.LogInfo($"{playerRole.Player.name}({playerRole.Player.Data.FriendCode})" +
                                        $" => {playerRole.Role.Name}");

                break;
        }
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void AfterPlayerFixedUpdate(PlayerFixedUpdateEvent @event)
    {
        var player = @event.Player;
        if (player == null! || !PlayerControl.LocalPlayer) return;
        if (GameStates.IsLobby && AmongUsClient.Instance.AmHost)
        {
            GameOptionsManager.Instance.currentNormalGameOptions.RoleOptions.SetRoleRate(RoleTypes.Scientist, 0, 0);
            GameOptionsManager.Instance.currentNormalGameOptions.RoleOptions.SetRoleRate(RoleTypes.Engineer, 0, 0);
            GameOptionsManager.Instance.currentNormalGameOptions.RoleOptions.SetRoleRate(RoleTypes.GuardianAngel, 0, 0);
            GameOptionsManager.Instance.currentNormalGameOptions.RoleOptions.SetRoleRate(RoleTypes.Shapeshifter, 0, 0);
        }

        if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId)
        {
            var role = player.GetRoleInstance();
            if (role is null) return;
            var text = player.cosmetics.nameText;
            text.color = role.Color;
            text.text = new StringBuilder().Append(role.Name).Append('\n').Append(player.Data.PlayerName).ToString();
        }
    }

    private static void SelectRoles()
    {
        GameUtils.PlayerRoleData.Clear(); // 首先清除 防止干扰

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
                role = Role.RoleManager.GetManager().GetTypeRoleInstance<Crewmate>(); // 无法分配默认职业为Crewmate
            }

            player.SetCustomRole(role);
        }

        // 打印职业分配信息
        foreach (var playerRole in GameUtils.PlayerRoleData)
            Main.Logger.LogInfo($"{playerRole.Player.name}({playerRole.Player.Data.FriendCode})" +
                                $" => {playerRole.Role.Name}");
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnSelectRoles(RoleManagerSelectRolesEvent @event)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        foreach (var playerRole in GameUtils.PlayerRoleData)
        {
            playerRole.Player.RpcSetRole(playerRole.Role.BaseRoleType);
        }
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnGameStart(GameStartManagerStartEvent @event)
    {
        var manager = @event.GameStartManager;
        if (HasStartedRoom)
            GameUtils.ForceClearGameData();
        else
            HasStartedRoom = true;
        // 改变按钮颜色
        manager.MakePublicButton.color = Palette.DisabledClear;
        manager.privatePublicText.color = Palette.DisabledClear;
    }

    [EventHandler(EventHandlerType.Prefix)]
    public bool OnMakePublic(GameStartManagerMakePublicEvent @event)
    {
        if (!AmongUsClient.Instance.AmHost) return false;
        GameUtils.SendGameMessage(LanguageConfig.Instance.MakePublicMessage);
        // 禁止设置为公开
        return false;
    }

    [EventHandler(EventHandlerType.Prefix)]
    public bool OnSetUpRoleText(IntroCutsceneShowRoleEvent @event)
    {
        var intro = @event.IntroCutscene;
        Main.Logger.LogInfo("Setup role text for the player...");

        var myRole = GameUtils.GetLocalPlayerRole();
        if (myRole == null) return true;

        var list = new List<IEnumerator>();

        void SetupRoles()
        {
            if (GameOptionsManager.Instance.currentGameMode != GameModes.Normal) return;
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

        list.Add(Effects.Action((Action)SetupRoles));
        list.Add(Effects.Wait(2.5f));

        void Action()
        {
            intro.YouAreText.gameObject.SetActive(false);
            intro.RoleText.gameObject.SetActive(false);
            intro.RoleBlurbText.gameObject.SetActive(false);
            intro.ourCrewmate.gameObject.SetActive(false);
        }

        list.Add(Effects.Action((Action)Action));

        @event.SetResult(Effects.Sequence(list.ToArray()));

        return false;
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnIntroDestroy(IntroCutsceneDestroyEvent @event)
    {
        var intro = @event.IntroCutscene;
        PlayerUtils.PoolablePlayerPrefab = Object.Instantiate(intro.PlayerPrefab);
        PlayerUtils.PoolablePlayerPrefab.gameObject.SetActive(false);
    }

    [EventHandler(EventHandlerType.Prefix)]
    public void OnSetUpTeamText(IntroCutsceneBeginCrewmateEvent @event)
    {
        var role = GameUtils.GetLocalPlayerRole();
        var player = PlayerControl.LocalPlayer;

        var camp = role?.CampType;
        if (camp is not (CampType.Neutral or CampType.Unknown)) return;
        var soloTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
        soloTeam.Add(player);
        @event.SetTeamToDisplay(soloTeam);
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void AfterSetUpTeamText(IntroCutsceneBeginCrewmateEvent @event)
    {
        var intro = @event.IntroCutscene;
        var role = GameUtils.GetLocalPlayerRole();

        if (role == null) return;
        var camp = role.CampType;

        intro.BackgroundBar.material.color = camp.GetColor();
        intro.TeamTitle.text = camp.GetName();
        intro.TeamTitle.color = camp.GetColor();

        intro.ImpostorText.text = camp.GetDescription();
    }

    [EventHandler(EventHandlerType.Prefix)]
    public bool OnCheckGameEnd(GameCheckEndEvent @event)
    {
        return !GlobalCustomOptionConstant.DebugMode.GetBool() && CustomWinnerManager.CheckEndForCustomWinners();
    }

    [EventHandler(EventHandlerType.Prefix)]
    public bool OnPlayerVent(VentCheckEvent @event)
    {
        var playerInfo = @event.PlayerInfo;
        foreach (var playerRole in GameUtils.PlayerRoleData)
        {
            if (!playerRole.Player.Data.IsSamePlayer(playerInfo)) continue;
            var ventAble = playerRole.Role.CanVent;
            @event.SetCanUse(ventAble);
            @event.SetCouldUse(ventAble);
            @event.SetResult(float.MaxValue);
            return false;
        }

        return true;
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnHudUpdate(HudManagerUpdateEvent @event)
    {
        var manager = @event.Manager;
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

    private static void ShareRoles()
    {
        var writer = RpcUtils.StartRpcImmediately(PlayerControl.LocalPlayer, (byte)KnownRpc.ShareRoles);

        var sb = new StringBuilder();

        for (var i = 0; i < GameUtils.PlayerRoleData.Count; i++)
        {
            var playerRole = GameUtils.PlayerRoleData[i];
            sb.Append(playerRole.Player.PlayerId + "|" + playerRole.Role.Id);

            if (i + 1 < GameUtils.PlayerRoleData.Count) sb.Append(',');
        }

        writer.Write(sb.ToString());

        // 职业格式应该是
        // playerId1|roleId1,playerId2|roleId2 

        writer.Finish();
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnBeginExile(PlayerExileBeginEvent @event)
    {
        var controller = @event.ExileController;
        var player = @event.Exiled?.Object;
        if (!player) return;

        var role = player!.GetRoleInstance();
        if (role == null) return;

        controller.completeString = role.HandleEjectText(player!);
    }
}