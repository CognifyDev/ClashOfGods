namespace COG.Listener.Event.Impl.TPBehaviour;

public class TaskPanelBehaviourSetTaskTextEvent : TaskPanelBehaviourEvent
{
    public TaskPanelBehaviourSetTaskTextEvent(TaskPanelBehaviour behaviour) : base(behaviour)
    {
    }

    public string GetTaskString()
    {
        return Behaviour.taskText.text;
    }

    public string SetTaskString(string newString)
    {
        return Behaviour.taskText.text = newString;
    }
}