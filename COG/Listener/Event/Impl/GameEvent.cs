namespace COG.Listener.Event.Impl;

public class GameEvent<T> : Event
{
    public T Object;

    public GameEvent(T obj)
    {
        Object = obj;
    }
}