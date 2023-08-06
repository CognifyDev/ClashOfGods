using COG.Listener;
using Reactor.Utilities.Extensions;
using System.Linq;
using System.Text;
using UnityEngine;

namespace COG.UI.CustomWinner
{
    class CustomWinner : IListener
    {
        public void OnGameEndSetEverythingUp(EndGameManager manager)
        {
            SetUpWinnerPlayers(manager);
            SetUpWinText(manager);
            SetUpRoleSummary(manager);
        }
        public static void SetUpWinnerPlayers(EndGameManager manager)
        {
            manager.transform.GetComponentsInChildren<PoolablePlayer>().ToList().ForEach(pb => pb.gameObject.Destroy());

            var sortedList = TempData.winners.ToArray().ToList().OrderBy(b => b.IsYou ? -1 : 0).ToList();
            int num = 0;
            int ceiling = Mathf.CeilToInt(7.5f);
            
            foreach (var winner in TempData.winners)
            {
                if (!(manager.PlayerPrefab && manager.transform)) break;
                
                var winnerPoolable = GameObject.Instantiate(manager.PlayerPrefab, manager.transform);
                var winnerControl = PlayerControl.AllPlayerControls.ToArray().Where(p => p.cosmetics.ColorId == winner.ColorId).FirstOrDefault();
                if (!winnerControl) continue;

                var winnerRole = Utils.PlayerUtils.GetRoleInstance(winnerControl!);
                if (winnerRole == null) continue;

                int offsetMultiplier = (num % 2 == 0) ? -1 : 1;
                int indexOffset = (num + 1) / 2;
                float lerpFactor = indexOffset / ceiling;
                float scaleLerp = Mathf.Lerp(1f, 0.75f, lerpFactor);
                float positionOffset = (num == 0) ? -8 : -1;

                winnerPoolable.transform.localPosition = new Vector3(offsetMultiplier * indexOffset * scaleLerp,
                    FloatRange.SpreadToEdges(-1.125f, 0f, indexOffset, ceiling),
                    positionOffset + indexOffset * 0.01f) * 0.9f;

                float scaleValue = Mathf.Lerp(1f, 0.65f, lerpFactor) * 0.9f;
                var scale = new Vector3(scaleValue, scaleValue, 1f);

                winnerPoolable.transform.localScale = scale;
                winnerPoolable.UpdateFromPlayerOutfit(winner, PlayerMaterial.MaskType.ComplexUI, winner.IsDead, true);

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

                winnerPoolable.SetName(winner.PlayerName + "\n" + winnerRole.Name);
                winnerPoolable.SetNameColor(winnerRole.Color);
                winnerPoolable.SetNamePosition(new(namePos.x, namePos.y, -15f));
                winnerPoolable.SetNameScale(new(1 / scale.x, 1 / scale.y, 1 / scale.z));

                num++;
            }
        }
        public static void SetUpWinText(EndGameManager manager)
        {
            var template = manager.WinText;
            var pos = template.transform.position;
            var winText = GameObject.Instantiate(template);

            winText.transform.position = new(pos.x, pos.y - 0.5f, pos.z);
            winText.transform.localScale = new(0.7f, 0.7f, 1f);
            winText.text = CustomWinnerManager.WinText;
            winText.color = CustomWinnerManager.WinColor;
            manager.BackgroundBar.material.SetColor("_Color", CustomWinnerManager.WinColor);

            // Reset
            CustomWinnerManager.WinText = "";
            CustomWinnerManager.WinColor = Color.white;
        }
        public static void SetUpRoleSummary(EndGameManager manager)
        {
            var position = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
            var roleSummary = GameObject.Instantiate(manager.WinText);

            roleSummary.transform.position = new Vector3(manager.Navigation.ExitButton.transform.position.x + 0.1f, position.y - 0.1f, -214f);
            roleSummary.transform.localScale = new Vector3(1f, 1f, 1f);
            roleSummary.fontSizeMax = roleSummary.fontSizeMin = roleSummary.fontSize = 1.5f;
            roleSummary.color = Color.white;

            StringBuilder summary = new("All players and their roles at the end of the game: \n");
            foreach(var role in Utils.PlayerRole.CachedRoles)
            {
                var deadPlayer = Utils.DeadPlayerManager.DeadPlayers.Where(dp => dp.PlayerId == role.PlayerId).FirstOrDefault();
                summary.Append(role.PlayerName).Append(' ').Append(role.Role.Name);
                summary.Append(' ').Append(Utils.ColorUtils.ToColorString(deadPlayer == null ? 
                    Palette.AcceptedGreen : 
                    Palette.ImpostorRed, 
                    deadPlayer == null ? 
                    "Alive" : 
                    (deadPlayer.DeathReason.ToString() ?? 
                    "Unknown")));
                summary.Append('\n');
            }

            roleSummary.text = summary.ToString();
        }
    }
}
