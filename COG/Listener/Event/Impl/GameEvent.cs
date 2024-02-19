namespace COG.Listener.Event.Impl;

public class GameEvent<T> : Event
{
    public T Object { get; }

    public GameEvent(T obj)
    {
        Object = obj;
    }
}