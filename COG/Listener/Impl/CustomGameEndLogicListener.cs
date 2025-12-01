using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using COG.Config.Impl;
using COG.Constant;
using COG.Game.CustomWinner;
using COG.Game.CustomWinner.Data;
using COG.Game.Events;
using COG.Listener.Event.Impl.AuClient;
using COG.Listener.Event.Impl.Game;
using COG.Listener.Event.Impl.Game.Record;
using COG.Role;
using COG.Rpc;
using COG.Utils;
using TMPro;
using UnityEngine;

namespace COG.Listener.Impl;

public class CustomGameEndLogicListener : IListener
{
    private static readonly int Color1 = Shader.PropertyToID("_Color");
    private readonly RpcHandler<byte[]> _winnerDataHandler;
    private bool _isEnding;

    public CustomGameEndLogicListener()
    {
        _winnerDataHandler = new RpcHandler<byte[]>(KnownRpc.ShareWinners,
            d =>
            {
                CustomWinnerManager.GetManager().WinnableData = new WinnableData();
                CustomWinnerManager.GetManager().WinnableData =
                    d.ToArray().DeserializeToData<SerializableWinnableData>().ToWinnableData();

                UpdateWinners();
            },
            (w, r) => w.WriteBytesAndSize(r), r => r.ReadBytesAndSize());

        IRpcHandler.Register(_winnerDataHandler);
    }

    public static bool DisableGameEndLogic { get; set; }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnGameStart(GameStartEvent _)
    {
        CustomWinnerManager.GetManager().InitForGameStart();
    }

    [EventHandler(EventHandlerType.Prefix)]
    [SuppressMessage("ReSharper", "PossibleLossOfFraction")]
    public bool OnCheckGameEnd(GameCheckEndEvent _)
    {
        if (_isEnding) return false;
        if (Input.GetKeyDown(KeyCode.F12))
        {
            var winnable = new WinnableData
            {
                GameOverReason = (GameOverReason)int.MaxValue
            };
            winnable.WinnablePlayers.AddRange(PlayerUtils.GetAllPlayers().Select(p => p.Data));
            PrepareForEndGame(winnable);

            return false;
        }

        if (GlobalCustomOptionConstant.DebugMode.GetBool() || !AmongUsClient.Instance.AmHost ||
            AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) return false;

        var winnableData = CustomWinnerManager.GetManager().CheckForGameEnd();
        if (!winnableData.Winnable) return false;

        PrepareForEndGame(winnableData);

        return false;

        void PrepareForEndGame(WinnableData data)
        {
            _isEnding = true;

            RpcSendWinnableData(CustomWinnerManager.GetManager().WinnableData);

            TaskUtils.RunTaskAfter(0.5f, () =>
                GameManager.Instance.RpcEndGame(data.GameOverReason,
                    false)); // Ensure each client has received & processed winnable data
        }

        void RpcSendWinnableData(WinnableData data)
        {
            _winnerDataHandler.Send(SerializableWinnableData.Of(data).SerializeToData()); // Just send, no performing
        }
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
    public void OnGameEndSetEverythingUp(GameSetEverythingUpEvent @event)
    {
        var manager = @event.Object;
        SetUpWinnerPlayers(manager);
        SetUpWinText(manager);
        SetUpRoleSummary(manager);

        GameStates.InRealGame = false;
        _isEnding = false;
    }

    private static void SetUpWinnerPlayers(EndGameManager manager)
    {
        manager.transform.GetComponentsInChildren<PoolablePlayer>().ToList()
            .ForEach(pb => pb.gameObject.TryDestroy());

        var num = 0;
        var ceiling = Mathf.CeilToInt(7.5f);

        var winners = CustomWinnerManager.GetManager().WinnableData.WinnablePlayers.Select(info
            => new CachedPlayerData(info)).ToList();
        Main.Logger.LogInfo($"Winners number => {winners.Count}");

        foreach (var winner in winners.ToArray().OrderBy(b => b.IsYou ? -1 : 0))
        {
            if (!(manager.PlayerPrefab && manager.transform)) break;

            var winnerPoolable = Object.Instantiate(manager.PlayerPrefab, manager.transform);
            if (winner == null!) continue;

            // Variable names optimization by ChatGPT
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
                $"Set up winner message for {winner.PlayerName} at {manager.transform.position}");

            num++;
        }

        manager.transform.GetComponentsInChildren<PoolablePlayer>().ForEach(p =>
        {
            var data = GameUtils.PlayerData.FirstOrDefault(d => d.ColorId == p.ColorId);
            if (data == null) return;
            p.ToggleName(true);

            var names = p.transform.FindChild("Names");
            names.localPosition = new Vector3(0, -0.7f, 0);
            var text = names.FindChild("NameText_TMP").GetComponent<TextMeshPro>();

            var subRoleNameBuilder = new StringBuilder();
            var subRoles = data.SubRoles;

            if (!subRoles.SequenceEqual([]))
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
        winText.color = data.WinColor;
        manager.BackgroundBar.material.SetColor(Color1, data.WinColor);
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

        var handler = LanguageConfig.Instance.GetHandler("game.survival-data");

        foreach (var player in GameUtils.PlayerData)
        {
            summary.Append(player.PlayerName.PadRight(10)).Append(' ')
                .Append(GetPlayerRoleChanges(player));

            summary.Append(' ').Append(GetPlayerAliveStateChanges(player));
            summary.Append(Environment.NewLine);
        }

        roleSummary.text = summary.ToString();

        string GetPlayerRoleChanges(CustomPlayerData player)
        {
            return player.MainRole.Color.ToColorString(player.MainRole.Name); // For future possible changes
        }

        string GetPlayerAliveStateChanges(CustomPlayerData player)
        {
            var gameEvents = EventRecorder.Instance.GetEvents().Where(e => player.Equals(e!.Player)!);
            var sortedEvents = gameEvents.OrderBy(e => e!.Time);
            var states = new List<string>();

            foreach (var gameEvent in sortedEvents)
                if (gameEvent is PlayerDisconnectGameEvent)
                {
                    states.Add(handler.GetString("disconnected").Color(Color.gray));
                }
                else if (gameEvent is PlayerExileGameEvent)
                {
                    states.Add(handler.GetString("exiled").Color(Palette.ImpostorRed));
                }
                else if (gameEvent is SheriffMisfireGameEvent)
                {
                    states.Add(handler.GetString("misfire").Color(Palette.ImpostorRed));
                }
                else if (gameEvent is PlayerReviveGameEvent)
                {
                    states.Add(handler.GetString("revival").Color(Palette.AcceptedGreen));
                }
                else if (gameEvent is PlayerDieGameEvent @event)
                {
                    var related = EventRecorder.Instance.GetEvents()
                        .OfType<PlayerKillGameEvent>()
                        .FirstOrDefault(e => e.Victim.Equals(player) && e.RelatedDeathEvent == @event);
                    if (related != null) states.Add(handler.GetString("default").Color(Palette.ImpostorRed));
                }

            if (!states.Any()) states.Add(handler.GetString("alive").Color(Palette.AcceptedGreen));

            return string.Join(" → ", states);
        }
    }
}