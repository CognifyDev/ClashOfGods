namespace COG.Listener.Impl;

public class WinnerListener : IListener
{
    public void AfterPlayerFixedUpdate(PlayerControl player)
    {
        foreach (var role in Role.RoleManager.GetManager().GetRoles())
        {
            var customWinner = role.CustomWinner;
            if (customWinner.CanWin)
            {
                GameManager.Instance.EndGame();
            }
        }
    }

    public void AfterGameEnd(AmongUsClient client, ref EndGameResult endGameResult)
    {
        
    }

    public void OnGameEnd(AmongUsClient client, ref EndGameResult endGameResult)
    {
        foreach (var role in Role.RoleManager.GetManager().GetRoles())
        {
            var customWinner = role.CustomWinner;
            if (customWinner.CanWin)
            {
                endGameResult.GameOverReason = customWinner.Peaceful ? GameOverReason.HumansByTask : GameOverReason.ImpostorByKill;
            }
        }
    }
}