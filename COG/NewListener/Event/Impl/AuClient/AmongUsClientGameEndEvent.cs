namespace COG.NewListener.Event.Impl.AUClient;

public class AmongUsClientGameEndEvent : AmongUsClientEvent
{
    private EndGameResult _endGameResult;
    
    public AmongUsClientGameEndEvent(AmongUsClient client, EndGameResult endGameResult) : base(client)
    {
        _endGameResult = endGameResult;
    }

    public void SetEndGameResult(EndGameResult result) => _endGameResult = result;

    public EndGameResult GetEndGameResult() => _endGameResult;
}