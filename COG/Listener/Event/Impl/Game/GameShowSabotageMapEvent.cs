namespace COG.Listener.Event.Impl.Game;

public class GameShowSabotageMapEvent : GameEvent<MapBehaviour>
{
    public GameShowSabotageMapEvent(MapBehaviour obj) : base(obj)
    {
    }
}