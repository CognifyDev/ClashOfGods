namespace COG.Listener.Event.Impl.TPBehaviour;

public class TaskPanelBehaviourSetTaskTextEvent : TaskPanelBehaviourEvent
{
    private string _taskString;
    public TaskPanelBehaviourSetTaskTextEvent(TaskPanelBehaviour behaviour) : base(behaviour)
    {
        _taskString = behaviour.taskText.text;
    }

    public string GetTaskString() => _taskString;
    public string SetTaskString(string newString) => _taskString = newString;
}