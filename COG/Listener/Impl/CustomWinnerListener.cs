using System;
using System.Linq;
using System.Text;
using COG.Config.Impl;
using COG.Game.CustomWinner;
using COG.Listener.Event.Impl.Game;
using COG.States;
using COG.Utils;
using UnityEngine;

namespace COG.Listener.Impl;

public class CustomWinnerListener : IListener
{
    private static readonly int Color1 = Shader.PropertyToID("_Color");

    [EventHandler(EventHandlerType.Postfix)]
    public void OnGameEndSetEverythingUp(GameSetEverythingUpEvent @event)
    {
        GameStates.InGame = false;

        var manager = @event.Object;
        SetUpWinnerPlayers(manager);
        SetUpWinText(manager);
        SetUpRoleSummary(manager);
    }

    // FIXME：小人没有身体，只有名字
    private static void SetUpWinnerPlayers(EndGameManager manager)
    {
        manager.transform.GetComponentsInChildren<PoolablePlayer>().ToList()
            .ForEach(pb => pb.gameObject.Destroy());

        var num = 0;
        var ceiling = Mathf.CeilToInt(7.5f);

        var winners = EndGameResult.CachedWinners;
        Main.Logger.LogInfo($"Winners number => {winners.Count}");

        foreach (var winner in winners.ToArray().OrderBy(b => b.IsYou ? -1 : 0))
        {
            if (!(manager.PlayerPrefab && manager.transform)) break;

            var winnerPoolable = Object.Instantiate(manager.PlayerPrefab, manager.transform);
            if (winner == null) continue;

            var winnerRole = PlayerRole.GetRole(winner.PlayerName);
            if (winnerRole == null!) continue;

            // ↓↓↓ These variables are from The Other Roles
            // Link: https://github.com/TheOtherRolesAU/TheOtherRoles/blob/main/TheOtherRoles/Patches/EndGamePatch.cs#L239
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
            winnerPoolable.UpdateFromPlayerOutfit(winner.Outfit, PlayerMaterial.MaskType.ComplexUI, winner.IsDead,
                true);

            if (winner.IsDead)
            {
                winnerPoolable.SetBodyAsGhost();
                winnerPoolable.SetDeadFlipX(num % 2 == 0);
            }
            else
            {
                winnerPoolable.SetFlipX(num % 2 == 0);
            }

            var namePos = winnerPoolable.cosmetics.nameText.transform.localPosition;

            winnerPoolable.SetName(winner.PlayerName + Environment.NewLine + winnerRole.Name);
            winnerPoolable.SetNameColor(winnerRole.Color);
            winnerPoolable.SetNamePosition(new Vector3(namePos.x, namePos.y, -15f));
            winnerPoolable.SetNameScale(new Vector3(1 / scale.x, 1 / scale.y, 1 / scale.z));

            Main.Logger.LogInfo(
                $"Set up winner message for {winner.PlayerName} at {manager.transform.position.ToString()}");

            num++;
        }
    }

    private static void SetUpWinText(EndGameManager manager)
    {
        var template = manager.WinText;
        var pos = template.transform.position;
        var winText = Object.Instantiate(template);

        winText.transform.position = new Vector3(pos.x, pos.y - 0.5f, pos.z);
        winText.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        winText.text = CustomWinnerManager.WinText;
        winText.color = CustomWinnerManager.WinColor;
        manager.BackgroundBar.material.SetColor(Color1, CustomWinnerManager.WinColor);

        // Reset
        CustomWinnerManager.SetWinText("");
        CustomWinnerManager.SetWinColor(Color.white);
        CustomWinnerManager.ResetWinningPlayers();
    }

    private static void SetUpRoleSummary(EndGameManager manager)
    {
        var position = Camera.main!.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
        var roleSummary = Object.Instantiate(manager.WinText);

        roleSummary.transform.position = new Vector3(manager.Navigation.ExitButton.transform.position.x + 0.1f,
            position.y - 0.1f, -214f);
        roleSummary.transform.localScale = new Vector3(1f, 1f, 1f);
        roleSummary.fontSizeMax = roleSummary.fontSizeMin = roleSummary.fontSize = 1.5f;
        roleSummary.color = Color.white;

        StringBuilder summary = new($"{LanguageConfig.Instance.ShowPlayersRolesMessage}");
        summary.Append(Environment.NewLine);
        foreach (var role in GameUtils.PlayerRoleData)
        {
            var deadPlayer = DeadPlayerManager.DeadPlayers.FirstOrDefault(dp => dp.PlayerId == role.PlayerId);
            summary.Append(role.PlayerName).Append(' ')
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