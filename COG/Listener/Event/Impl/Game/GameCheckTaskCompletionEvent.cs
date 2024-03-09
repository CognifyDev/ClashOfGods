namespace COG.Listener.Event.Impl.Game;

public class GameCheckTaskCompletionEvent : GameEvent<GameManager>
{
    private bool _result;

    public GameCheckTaskCompletionEvent(GameManager obj, bool result) : base(obj)
    {
        _result = result;
    }

    public void SetResult(bool result)
    {
        _result = result;
    }

    public bool GetResult()
    {
        return _result;
    }
}