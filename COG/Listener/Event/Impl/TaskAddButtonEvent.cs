namespace COG.Listener.Event.Impl;

public class TaskAddButtonEvent : Event
{
    public TaskAddButtonEvent(TaskAddButton taskAddButton)
    {
        TaskAddButton = taskAddButton;
    }

    public TaskAddButton TaskAddButton { get; }
}