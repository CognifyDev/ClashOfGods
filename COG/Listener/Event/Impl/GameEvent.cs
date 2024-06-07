namespace COG.Listener.Event.Impl;

public class GameEvent<T> : Event
{
    public GameEvent(T obj)
    {
        Object = obj;
    }

    public T Object { get; }
}