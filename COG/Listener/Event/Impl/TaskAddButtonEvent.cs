namespace COG.Listener.Event.Impl;

public class TaskAddButtonEvent : Listener.Event.Event
{
    public TaskAddButton TaskAddButton { get; }
    
    public TaskAddButtonEvent(TaskAddButton taskAddButton)
    {
        TaskAddButton = taskAddButton;
    }
}