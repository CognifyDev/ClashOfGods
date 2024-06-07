namespace COG.Listener.Event.Impl;

public class TaskAdderGameEvent : Event
{
    public TaskAdderGameEvent(TaskAdderGame taskAdderGame)
    {
        TaskAdderGame = taskAdderGame;
    }

    public TaskAdderGame TaskAdderGame { get; }
}