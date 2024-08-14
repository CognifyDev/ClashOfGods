using System;
using System.Linq;
using System.Text;
using COG.Config.Impl;
using COG.Constant;
using COG.Game.CustomWinner;
using COG.Listener.Event.Impl.AuClient;
using COG.Listener.Event.Impl.Game;
using COG.Role;
using COG.States;
using COG.Utils;
using TMPro;
using UnityEngine;

namespace COG.Listener.Impl;

public class CustomWinnerListener : IListener
{
    private static readonly int Color1 = Shader.PropertyToID("_Color");
    
    [EventHandler(EventHandlerType.Postfix)]
    public void OnGameStart(GameStartEvent _)
    {
        CustomWinnerManager.GetManager().InitForGameStart();
    }

    [EventHandler(EventHandlerType.Prefix)]
    public bool OnCheckGameEnd(GameCheckEndEvent @event)
    {
        if (GlobalCustomOptionConstant.DebugMode.GetBool()) return false;
        var checkForGameEnd = CustomWinnerManager.GetManager().CheckForGameEnd();
        if (checkForGameEnd.Winnable)
            GameManager.Instance.RpcEndGame(checkForGameEnd.GameOverReason, false);
        return false;
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnAmongUsClientGameEnd(AmongUsClientGameEndEvent @event)
    {
        var endGameResult = @event.GetEndGameResult();
        var data = CustomWinnerManager.GetManager().WinnableData;
        EndGameResult.CachedWinners.Clear();
        EndGameResult.CachedWinners = data.WinnablePlayers
            .Select(player => new CachedPlayerData(player.Data)).ToList().ToIl2CppList();
        endGameResult.GameOverReason = data.GameOverReason;
        EndGameResult.CachedGameOverReason = data.GameOverReason;
    }
    
    [EventHandler(EventHandlerType.Postfix)]
    public void OnGameEndSetEverythingUp(GameSetEverythingUpEvent @event)
    {
        var manager = @event.Object;
        SetUpWinnerPlayers(manager);
        SetUpWinText(manager);
        SetUpRoleSummary(manager);
        
        GameStates.InGame = false;
    }

    private static void SetUpWinnerPlayers(EndGameManager manager)
    {
        Main.Logger.LogInfo(EndGameResult.CachedWinners.ToArray().Select(winner => winner.PlayerName)
            .ToArray().AsString());
        manager.transform.GetComponentsInChildren<PoolablePlayer>().ForEach(p =>
        {
            var data = GameUtils.PlayerData.FirstOrDefault(d => d.ColorId == p.ColorId);
            if (data == null) return;
            p.ToggleName(true);

            var names = p.transform.FindChild("Names");
            names.localPosition = new(0, -0.7f, 0);
            var text = names.FindChild("NameText_TMP").GetComponent<TextMeshPro>();

            var subRoleNameBuilder = new StringBuilder();
            var subRoles = data.SubRoles;

            if (!subRoles.SequenceEqual(Array.Empty<CustomRole>()))
                foreach (var role in subRoles)
                    subRoleNameBuilder.Append(' ').Append(role.GetColorName());

            text.text = $"{data.PlayerName}\n{data.Role.Name}{subRoleNameBuilder}";
            text.color = data.Role.Color;
        });
    }

    private static void SetUpWinText(EndGameManager manager)
    {
        var data = CustomWinnerManager.GetManager().WinnableData;
        var template = manager.WinText;
        var pos = template.transform.position;
        var winText = Object.Instantiate(template);

        winText.name = "TeamWinText";
        winText.transform.position = new Vector3(pos.x, pos.y - 0.5f, pos.z);
        winText.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        winText.text = data.WinText ?? "";
        if (data.WinColor != null)
        {
            winText.color = data.WinColor.Value;
            manager.BackgroundBar.material.SetColor(Color1, data.WinColor.Value);
        }
    }

    private static void SetUpRoleSummary(EndGameManager manager)
    {
        var position = Camera.main!.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
        var roleSummary = Object.Instantiate(manager.WinText);

        roleSummary.name = "RoleSummary";
        roleSummary.transform.position = new Vector3(-0.5f,
            position.y - 0.1f, -214f);
        roleSummary.transform.localScale = new Vector3(1f, 1f, 1f);
        roleSummary.fontSizeMax = roleSummary.fontSizeMin = roleSummary.fontSize = 1.5f;
        roleSummary.color = Color.white;
        roleSummary.alignment = TextAlignmentOptions.TopLeft;

        var summary = new StringBuilder($"{LanguageConfig.Instance.ShowPlayersRolesMessage}\n");
        summary.Append(Environment.NewLine);
        foreach (var role in GameUtils.PlayerData)
        {
            var deadPlayer = DeadPlayerManager.DeadPlayers.FirstOrDefault(dp => dp.PlayerId == role.PlayerId);
            summary.Append(role.PlayerName.PadRight(10)).Append(' ')
                .Append(role.Role.Color.ToColorString(role.Role.Name));
            summary.Append(' ').Append((deadPlayer == null ? Palette.AcceptedGreen : Palette.ImpostorRed).ToColorString(
                deadPlayer == null ? LanguageConfig.Instance.Alive :
                deadPlayer.DeathReason == null ? LanguageConfig.Instance.UnknownKillReason :
                deadPlayer.DeathReason.GetLanguageDeathReason()));
            summary.Append(Environment.NewLine);
        }

        roleSummary.text = summary.ToString();
    }
}