using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using COG.Config.Impl;
using COG.Constant;
using COG.Game.CustomWinner;
using COG.Game.CustomWinner.Data;
using COG.Listener.Event.Impl.AuClient;
using COG.Listener.Event.Impl.Game;
using COG.Listener.Event.Impl.Player;
using COG.Role;
using COG.Rpc;
using COG.States;
using COG.Utils;
using TMPro;
using UnityEngine;

namespace COG.Listener.Impl;

public class CustomWinnerListener : IListener
{
    private bool _ending;
    
    private static readonly int Color1 = Shader.PropertyToID("_Color");
    
    [EventHandler(EventHandlerType.Postfix)]
    public void OnGameStart(GameStartEvent _)
    {
        CustomWinnerManager.GetManager().InitForGameStart();
    }

    [EventHandler(EventHandlerType.Prefix)]
    [SuppressMessage("ReSharper", "PossibleLossOfFraction")]
    public bool OnCheckGameEnd(GameCheckEndEvent _)
    {
        if (_ending) return false;
        if (GlobalCustomOptionConstant.DebugMode.GetBool() || !AmongUsClient.Instance.AmHost) return false;
        var checkForGameEnd = CustomWinnerManager.GetManager().CheckForGameEnd();
        if (!checkForGameEnd.Winnable) return false;
        _ending = true;
        var data = CustomWinnerManager.GetManager().WinnableData;
        var writer = RpcUtils.StartRpcImmediately(PlayerControl.LocalPlayer, KnownRpc.ShareWinners);
        writer.WriteBytesAndSize(SerializableWinnableData.Of(data).SerializeToData());
        writer.Finish();
            
        TaskUtils.RunTaskAfter(1.5f, () =>
            GameManager.Instance.RpcEndGame(checkForGameEnd.GameOverReason, false));
        return false;
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnAmongUsClientGameEnd(AmongUsClientGameEndEvent @event)
    {
        UpdateWinners();
    }

    private static void UpdateWinners()
    {
        var data = CustomWinnerManager.GetManager().WinnableData;
        EndGameResult.CachedGameOverReason = data.GameOverReason;
        EndGameResult.CachedWinners.Clear();
        EndGameResult.CachedWinners = CustomWinnerManager.GetManager()
            .WinnableData.GetWinnablePlayersAsCachedPlayerData().ToIl2CppList();
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnRpcReceived(PlayerHandleRpcEvent @event)
    {
        if (!GameStates.InGame) return;
        if (@event.CallId != (byte)KnownRpc.ShareWinners) return;
        
        CustomWinnerManager.GetManager().WinnableData = new WinnableData();
        CustomWinnerManager.GetManager().WinnableData = @event.Reader.ReadBytesAndSize().ToArray()
            .DeserializeToData<SerializableWinnableData>().ToWinnableData();
            
        UpdateWinners();
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnGameEndSetEverythingUp(GameSetEverythingUpEvent @event)
    {
        var manager = @event.Object;
        SetUpWinnerPlayers(manager);
        SetUpWinText(manager);
        SetUpRoleSummary(manager);
        
        GameStates.InGame = false;
        _ending = false;
    }

    private static void SetUpWinnerPlayers(EndGameManager manager)
    {
        manager.transform.GetComponentsInChildren<PoolablePlayer>().ToList()
            .ForEach(pb => pb.gameObject.Destroy());

        var num = 0;
        var ceiling = Mathf.CeilToInt(7.5f);

        var winners = CustomWinnerManager.GetManager().WinnableData.WinnablePlayers.Select(info 
            => new CachedPlayerData(info)).ToList();
        Main.Logger.LogDebug($"Winners number => {winners.Count}");

        foreach (var winner in winners.ToArray().OrderBy(b => b.IsYou ? -1 : 0))
        {
            if (!(manager.PlayerPrefab && manager.transform)) break;

            var winnerPoolable = Object.Instantiate(manager.PlayerPrefab, manager.transform);
            if (winner == null!) continue;
            
            // Variable names optimizing by ChatGPT
            var offsetMultiplier = num % 2 == 0 ? -1 : 1;
            var indexOffset = (num + 1) / 2;
            var lerpFactor = indexOffset / ceiling;
            var scaleLerp = Mathf.Lerp(1f, 0.75f, lerpFactor);
            float positionOffset = num == 0 ? -8 : -1;

            winnerPoolable.transform.localPosition = new Vector3(offsetMultiplier * indexOffset * scaleLerp,
                FloatRange.SpreadToEdges(-1.125f, 0f, indexOffset, ceiling),
                positionOffset + indexOffset * 0.01f) * 0.9f;

            var scaleValue = Mathf.Lerp(1f, 0.65f, lerpFactor) * 0.9f;
            var scale = new Vector3(scaleValue, scaleValue, 1f);

            winnerPoolable.transform.localScale = scale;

            if (winner.IsDead)
            {
                winnerPoolable.SetBodyAsGhost();
                winnerPoolable.SetDeadFlipX(num % 2 == 0);
            }
            else
            {
                winnerPoolable.SetFlipX(num % 2 == 0);
            }
            
            winnerPoolable.UpdateFromPlayerOutfit(winner.Outfit, PlayerMaterial.MaskType.None, winner.IsDead, true);

            var namePos = winnerPoolable.cosmetics.nameText.transform.localPosition;
            
            winnerPoolable.SetNamePosition(new Vector3(namePos.x, namePos.y, -15f));
            winnerPoolable.SetNameScale(new Vector3(1 / scale.x, 1 / scale.y, 1 / scale.z));

            Main.Logger.LogDebug(
                $"Set up winner message for {winner.PlayerName} at {manager.transform.position.ToString()}");

            num++;
        }
        
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

            text.text = $"{data.PlayerName}\n{data.MainRole.Name}{subRoleNameBuilder}";
            text.color = data.MainRole.Color;
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
                .Append(role.MainRole.Color.ToColorString(role.MainRole.Name));
            summary.Append(' ').Append((deadPlayer == null ? Palette.AcceptedGreen : Palette.ImpostorRed).ToColorString(
                deadPlayer == null ? LanguageConfig.Instance.Alive :
                deadPlayer.DeathReason == null ? LanguageConfig.Instance.UnknownKillReason :
                deadPlayer.DeathReason.GetLanguageDeathReason()));
            summary.Append(Environment.NewLine);
        }

        roleSummary.text = summary.ToString();
    }
}