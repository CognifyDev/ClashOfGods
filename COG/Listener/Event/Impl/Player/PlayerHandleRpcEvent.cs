namespace COG.Listener.Event.Impl.Player;

public class PlayerHandleRpcEvent : PlayerEvent
{
    public byte CallId { get; }
    public MessageReader MessageReader { get; }

    public PlayerHandleRpcEvent(PlayerControl player, byte callId, MessageReader reader) : base(player)
    {
        CallId = callId;
        MessageReader = reader;
    }
}