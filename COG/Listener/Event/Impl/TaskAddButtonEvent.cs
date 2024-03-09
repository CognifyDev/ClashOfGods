namespace COG.Listener.Event.Impl;

public class TaskAddButtonEvent : Event
{
    public TaskAddButton TaskAddButton { get; }

    public TaskAddButtonEvent(TaskAddButton taskAddButton)
    {
        TaskAddButton = taskAddButton;
    }
}