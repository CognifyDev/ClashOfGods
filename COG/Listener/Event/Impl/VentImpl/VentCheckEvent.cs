namespace COG.Listener.Event.Impl.VentImpl;

public class VentCheckEvent : VentEvent
{
    public GameData.PlayerInfo PlayerInfo { get; }
    private bool _canUse, _couldUse;
    private float _result;
    
    public VentCheckEvent(Vent vent, GameData.PlayerInfo playerInfo, bool canUse, bool couldUse, float result) : base(vent)
    {
        PlayerInfo = playerInfo;
        _canUse = canUse;
        _couldUse = couldUse;
        _result = result;
    }

    public void SetCanUse(bool canUse) => _canUse = canUse;

    public bool GetCanUse() => _canUse;

    public void SetCouldUse(bool couldUse) => _couldUse = couldUse;

    public bool GetCouldUse() => _couldUse;

    public void SetResult(float result) => _result = result;

    public float GetResult() => _result;
}