using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Constant;
using COG.Listener.Event;
using COG.Listener.Event.Impl.Game;
using COG.Listener.Event.Impl.GSManager;
using COG.Listener.Event.Impl.HManager;
using COG.Listener.Event.Impl.ICutscene;
using COG.Listener.Event.Impl.Player;
using COG.Listener.Event.Impl.RManager;
using COG.Listener.Event.Impl.VentImpl;
using COG.Role;
using COG.Role.Impl.Crewmate;
using COG.Role.Impl.Impostor;
using COG.Rpc;
using COG.UI.CustomGameObject.Arrow;
using COG.Utils;
using Il2CppSystem.Collections;
using InnerNet;
using UnityEngine;
using Action = Il2CppSystem.Action;
using GameStates = COG.States.GameStates;
using Random = System.Random;
using COG.UI.CustomButton;

namespace COG.Listener.Impl;

public class GameListener : IListener
{
    [EventHandler(EventHandlerType.Prefix)]
    public void OnRpcReceived(PlayerHandleRpcEvent @event)
    {
        var callId = @event.CallId;
        var reader = @event.Reader;
        var knownRpc = (KnownRpc)callId;

        switch (knownRpc)
        {
            case KnownRpc.ShareRoles:
            {
                if (AmongUsClient.Instance.AmHost) return;
                // 清除原列表，防止干扰
                GameUtils.PlayerData.Clear();
                // 开始读入数据
                Main.Logger.LogDebug("The role data from the host has been received.");

                var count = reader.ReadPackedInt32();

                for (var i = 0; i < count; i++)
                {
                    var bytes = reader.ReadBytesAndSize().ToArray();
                    var data = bytes.DeserializeToData<SerializablePlayerData>().AsPlayerData();
                    GameUtils.PlayerData.Add(data);
                    data.Player.SetCustomRole(data.MainRole, data.SubRoles);
                }
                
                foreach (var playerRole in GameUtils.PlayerData)
                    Main.Logger.LogInfo($"{playerRole.Player.Data.PlayerName}({playerRole.Player.Data.FriendCode})" +
                                        $" => {playerRole.MainRole.GetNormalName()}{playerRole.SubRoles.AsString()}");

                break;
            }
            case KnownRpc.Revive:
            {
                // 从Rpc中读入PlayerControl
                var target = reader.ReadNetObject<PlayerControl>();
                
                // 复活目标玩家
                target.Revive();
                break;
            }
            case KnownRpc.CleanDeadBody:
            {
                if (!GameStates.InRealGame) return;
                var pid = reader.ReadByte();
                var body = Object.FindObjectsOfType<DeadBody>().ToList().FirstOrDefault(b => b.ParentId == pid);
                if (!body) return;
                body!.gameObject.SetActive(false);
                break;
            }
            case KnownRpc.Mark:
            {
                var target = reader.ReadNetObject<PlayerControl>();
                var tag = reader.ReadString();
                var playerData = target.GetPlayerData();

                if (tag.StartsWith(PlayerUtils.DeleteTagPrefix))
                {
                    playerData?.Tags.Remove(tag.Replace(PlayerUtils.DeleteTagPrefix, ""));
                    break;
                }
                
                playerData?.Tags.Add(tag);
                break;
            }
        }
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void AfterPlayerFixedUpdate(PlayerFixedUpdateEvent @event)
    {
        var player = @event.Player;
        if (player == null! || !PlayerControl.LocalPlayer) return;
        if (GameStates.InLobby && AmongUsClient.Instance.AmHost)
        {
            var mainOption = GameUtils.GetGameOptions();
            var roleOption = mainOption.RoleOptions;

            var changed = false;

            foreach (var role in Enum.GetValues<RoleTypes>())
            {
                if (roleOption.GetNumPerGame(role) != 0 || roleOption.GetChancePerGame(role) != 0)
                {
                    roleOption.SetRoleRate(role, 0, 0);
                    changed = true;
                }
            }

            if (mainOption.RulesPreset != RulesPresets.Custom)
            {
                mainOption.RulesPreset = RulesPresets.Custom;
                changed = true;
            }

            if (changed)
            {
                GameManager.Instance.LogicOptions.SyncOptions();
            }
        }

        var playersToDisplayInfo = new List<PlayerControl>() { PlayerControl.LocalPlayer };

        if (!PlayerControl.LocalPlayer.IsAlive())
            playersToDisplayInfo = PlayerControl.AllPlayerControls.ToArray().ToList();

        playersToDisplayInfo.ForEach(p => p.DisplayPlayerInfoOnName());
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
            givenTimes ++;
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
        foreach (var playerRole in GameUtils.PlayerData)
            Main.Logger.LogInfo($"{playerRole.Player.name}({playerRole.Player.Data.FriendCode})" +
                                $" => {playerRole.MainRole.GetNormalName()}" +
                                $"{playerRole.SubRoles.Select(subRole => subRole.GetNormalName()).ToList().AsString()}");
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnSelectRoles(RoleManagerSelectRolesEvent @event)
    {
        Main.Logger.LogInfo("Select roles for players...");
        SelectRoles();

        Main.Logger.LogInfo("Share roles for players...");
        ShareRoles();

        var roleList = GameUtils.PlayerData.Select(pr => pr.MainRole).ToList();
        roleList.AddRange(GameUtils.PlayerData.SelectMany(pr => pr.SubRoles));

        foreach (var availableRole in roleList) availableRole.AfterSharingRoles();
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnJoinLobby(GameStartManagerStartEvent @event)
    {
        var manager = @event.GameStartManager;
        var privateButton = manager.HostPrivacyButtons.transform.FindChild("PRIVATE BUTTON");
        var inactive = privateButton.FindChild("Inactive").GetComponent<SpriteRenderer>();
        var highlight = privateButton.FindChild("Highlight").GetComponent<SpriteRenderer>();
        inactive.color = highlight.color = Palette.DisabledGrey;
    }

    [EventHandler(EventHandlerType.Prefix)]
    public bool OnMakePublic(GameStartManagerMakePublicEvent _)
    {
        if (!AmongUsClient.Instance.AmHost) return false;
        GameUtils.SendGameMessage(LanguageConfig.Instance.MakePublicMessage);
        // 禁止设置为公开
        return false;
    }

    [EventHandler(EventHandlerType.Prefix)]
    public bool OnSetUpRoleText(IntroCutsceneShowRoleEvent @event)
    {
        var intro = @event.IntroCutscene!;
        Main.Logger.LogInfo("Set up role text for the player...");

        var myRole = GameUtils.GetLocalPlayerRole();

        var list = new List<IEnumerator>
        {
            Effects.Action((Action)SetUpRoles),
            Effects.Wait(2.5f)
        };

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

        void SetUpRoles()
        {
            if (GameOptionsManager.Instance.currentGameMode != GameModes.Normal) return;
            intro.RoleText.text = myRole.Name;
            intro.RoleText.color = myRole.Color;

            var sb = new StringBuilder(myRole.GetColorName());
            foreach (var sub in PlayerControl.LocalPlayer.GetSubRoles())
                sb.Append(" + ").Append(sub.GetColorName());

            intro.YouAreText.text = sb.ToString();
            intro.YouAreText.color = myRole.Color;
            intro.RoleBlurbText.color = myRole.Color;
            intro.RoleBlurbText.text = myRole.ShortDescription;

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

    [EventHandler(EventHandlerType.Prefix)]
    public void OnSetUpTeamText(IntroCutsceneBeginCrewmateEvent @event)
    {
        var role = GameUtils.GetLocalPlayerRole();
        var player = PlayerControl.LocalPlayer;

        var camp = role.CampType;
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

        var camp = role.CampType;

        intro.BackgroundBar.material.color = camp.GetColor();
        intro.TeamTitle.text = camp.GetName();
        intro.TeamTitle.color = camp.GetColor();

        intro.ImpostorText.text = camp.GetDescription();
    }

    [EventHandler(EventHandlerType.Prefix)]
    public bool OnVentCheck(VentCheckEvent @event)
    {
        var playerInfo = @event.PlayerInfo;
        if (playerInfo.Disconnected) return true;

        var role = playerInfo.Object.GetMainRole();

        if (!role.CanVent)
        {
            @event.SetCanUse(false);
            @event.SetCouldUse(false);
            @event.SetResult(float.MaxValue);
            return false;
        }
        else
        {
            var usable = @event.GetCanUse() && @event.GetCouldUse();
            if (usable)
            {
                var vent = @event.Vent;
                vent.myRend.material.SetFloat("_Outline", usable ? 1 : 0);
                vent.myRend.material.SetColor("_OutlineColor", role.Color);
            }
        }

        return true;
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnHudUpdate(HudManagerUpdateEvent @event)
    {
        var manager = @event.Manager;
        var role = GameUtils.GetLocalPlayerRole();

        if (!role.CanVent)
        {
            manager.ImpostorVentButton.SetDisabled();
            manager.ImpostorVentButton.ToggleVisible(false);
        }

        if (!role.CanSabotage)
        {
            manager.SabotageButton.SetDisabled();
            manager.SabotageButton.ToggleVisible(false);
        }

        Arrow.CreatedArrows.RemoveAll(a => !a.ArrowObject);
        Arrow.CreatedArrows.ForEach(a => a.Update());

        var hint = GameObject.Find("RoleHintTask");
        if (!hint) return;

        hint.GetComponent<ImportantTextTask>().Text = GetRoleHintText();
    }

    private static void ShareRoles()
    {
        var writer = RpcUtils.StartRpcImmediately(PlayerControl.LocalPlayer, KnownRpc.ShareRoles);

        writer.WritePacked(GameUtils.PlayerData.Count);
        
        foreach (var bytes in GameUtils.PlayerData.Select(SerializablePlayerData.Of).Select(data => data.SerializeToData()))
            writer.WriteBytesAndSize(bytes);
        
        writer.Finish();
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnBeginExile(PlayerExileBeginEvent @event)
    {
        if (!GameUtils.GetGameOptions().ConfirmImpostor) return;

        var controller = @event.ExileController;
        var state = @event.ExileState;
        var exiled = state.networkedPlayer;

        var crewCount = GetCount(PlayerUtils.AllCrewmates);
        var impCount = GetCount(PlayerUtils.AllImpostors);
        var neutralCount = GetCount(PlayerUtils.AllNeutrals);
        
        if (!state.voteTie && exiled && (state.confirmImpostor || PlayerControl.LocalPlayer.Data.IsDead))
        {
            var role = exiled.GetMainRole();
            controller.completeString = role.HandleEjectText(state.networkedPlayer);
        }
        
        controller.ImpostorText.text =
            LanguageConfig.Instance.AlivePlayerInfo.CustomFormat(crewCount, neutralCount, impCount);
        return;

        int GetCount(IEnumerable<PlayerData> list)
        {
            return list.Select(p => p.Data)
                .Count(p => !p.IsDead);
        }
    }

    private readonly List<Handler> _handlers = new();

    [EventHandler(EventHandlerType.Postfix)]
    public void OnGameStart(GameStartEvent _)
    {
        Main.Logger.LogInfo("Game started!");

        GameStates.InRealGame = true;
        
        if (!_handlers.IsEmpty())
        {
            ListenerManager.GetManager().UnRegisterHandlers(_handlers.ToArray());
            _handlers.Clear();
        }
        
        CustomRoleManager.GetManager().GetRoles().Select(role => role.GetListener()).ForEach(
            listener => _handlers.AddRange(ListenerManager.GetManager().AsHandlers(listener)));

        CustomButton.ResetAllCooldown();

        ListenerManager.GetManager().RegisterHandlers(_handlers.ToArray());

        var impText = Object.FindObjectOfType<ImportantTextTask>();
        if (!impText)
            impText = PlayerTask.GetOrCreateTask<ImportantTextTask>(PlayerControl.LocalPlayer);

        impText.name = "RoleHintTask";
        impText.Text = GetRoleHintText();

        if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
            foreach (var player in PlayerControl.AllPlayerControls)
                player.RpcSetCustomRole<Crewmate>();
    }

    private string GetRoleHintText()
    {
        var localRole = GameUtils.GetLocalPlayerRole();
        bool isImpostorRole = localRole.CampType == CampType.Impostor;

        var sb = new StringBuilder();

        sb.Append(localRole.GetColorName()).Append("：".Color(localRole.Color))
            .Append(localRole.ShortDescription.Color(localRole.Color)).Append("\r\n\r\n");

        if (isImpostorRole)
            sb.Append($"<color=#FF1919FF>{TranslationController.Instance.GetString(StringNames.FakeTasks)}</color>\n");
        
        return sb.ToString();
    }
}