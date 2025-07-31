using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Listener.Event;
using COG.Listener.Event.Impl.Game;
using COG.Listener.Event.Impl.HManager;
using COG.Listener.Event.Impl.Player;
using COG.Listener.Event.Impl.VentImpl;
using COG.Patch;
using COG.Role;
using COG.UI.CustomButton;
using COG.UI.CustomGameObject.Arrow;
using COG.UI.CustomGameObject.HudMessage;
using COG.Utils;
using COG.Utils.Coding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace COG.Listener.Impl;

public class GameListener : IListener
{
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
        {
            playersToDisplayInfo = PlayerControl.AllPlayerControls.ToArray().ToList();
            playersToDisplayInfo.ForEach(p => p.DisplayPlayerInfoOnName());
        }
        else
        {
            PlayerControl.AllPlayerControls.ToArray().Do(p => p.DisplayPlayerInfoOnName(true));
            PlayerControl.LocalPlayer.DisplayPlayerInfoOnName(); // display complete info
        }
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

        var hint = GameObject.Find("RoleHintTask");
        if (!hint) return;

        hint.GetComponent<ImportantTextTask>().Text = GetRoleHintText();
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
            LanguageConfig.Instance.AlivePlayerInfo.CustomFormat(("crew", crewCount), ("neutral", neutralCount), ("imp", impCount));
        return;

        int GetCount(IEnumerable<CustomPlayerData> list)
        {
            return list.Select(p => p.Data)
                .Count(p => !p.IsDead);
        }
    }

    private readonly List<Handler> _handlers = new();

    [EventHandler(EventHandlerType.Postfix)]
    [FixMe("Once died, always show role info even if revived")]
    public void OnGameStart(GameStartEvent _)
    {
        Main.Logger.LogInfo("========Game Starts!========");

        GameStates.InRealGame = true;
        
        Main.Logger.LogInfo("Registering game listeners...");

        if (!_handlers.IsEmpty())
        {
            ListenerManager.GetManager().UnRegisterHandlers(_handlers.ToArray());
            _handlers.Clear();
        }
        
        CustomRoleManager.GetManager().GetRoles().Select(role => role.GetListener()).ForEach(
            listener => _handlers.AddRange(ListenerManager.GetManager().AsHandlers(listener)));

        ListenerManager.GetManager().RegisterHandlers(_handlers.ToArray());

        Main.Logger.LogInfo("Resetting gameplay objects...");
        CustomButton.ResetAllCooldown();
        VanillaKillButtonPatch.Initialize();

        var impText = PlayerTask.GetOrCreateTask<ImportantTextTask>(PlayerControl.LocalPlayer);

        impText.name = "RoleHintTask";
        impText.Text = GetRoleHintText();

        new GameObject("HudMessageManager").AddComponent<HudMessage>();

        if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
            foreach (var player in PlayerControl.AllPlayerControls)
                player.RpcSetCustomRole<Crewmate>();
    }

    private static string GetRoleHintText()
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