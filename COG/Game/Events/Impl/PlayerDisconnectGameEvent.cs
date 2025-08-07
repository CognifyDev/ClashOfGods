using COG.Utils;

namespace COG.Game.Events.Impl;

public class PlayerDisconnectGameEvent : GameEventBase
{
    public PlayerDisconnectGameEvent(CustomPlayerData player) : base(GameEventType.Disconnect, player)
    {
    }
}