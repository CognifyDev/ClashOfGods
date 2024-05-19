namespace COG.Listener.Event.Impl.TPBehaviour;

public class TaskPanelBehaviourSetTaskTextEvent : TaskPanelBehaviourEvent
{
    public TaskPanelBehaviourSetTaskTextEvent(TaskPanelBehaviour behaviour) : base(behaviour)
    {
    }

    public string GetTaskString() => Behaviour.taskText.text;
    public string SetTaskString(string newString) => Behaviour.taskText.text = newString;
}