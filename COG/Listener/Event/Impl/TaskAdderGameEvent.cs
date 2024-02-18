namespace COG.Listener.Event.Impl;

public class TaskAdderGameEvent : Listener.Event.Event
{
    public TaskAdderGame TaskAdderGame { get; }
    
    public TaskAdderGameEvent(TaskAdderGame taskAdderGame)
    {
        TaskAdderGame = taskAdderGame;
    }
}