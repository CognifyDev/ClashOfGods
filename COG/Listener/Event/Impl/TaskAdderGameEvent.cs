namespace COG.Listener.Event.Impl;

public class TaskAdderGameEvent : Event
{
    public TaskAdderGame TaskAdderGame { get; }

    public TaskAdderGameEvent(TaskAdderGame taskAdderGame)
    {
        TaskAdderGame = taskAdderGame;
    }
}