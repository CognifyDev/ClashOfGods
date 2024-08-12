using COG.Game.CustomWinner;
using COG.Game.CustomWinner.Data;
using COG.Listener.Event.Impl.Game;

namespace COG.Listener.Impl;

public class CustomWinnerListener : IListener
{
    [EventHandler(EventHandlerType.Postfix)]
    public void OnGameStart(GameStartEvent _)
    {
        CustomWinnerManager.GetManager().InitForGameStart();
    }

    [EventHandler(EventHandlerType.Prefix)]
    public bool OnCheckGameEnd(GameCheckEndEvent @event)
    {
        CheckForWinnableData(CustomWinnerManager.GetManager().CheckForGameEnd());
        return false;
    }

    private void CheckForWinnableData(WinnableData data)
    {
        if (!data.Winnable) return;
        // TODO 游戏结束分配
    }
}