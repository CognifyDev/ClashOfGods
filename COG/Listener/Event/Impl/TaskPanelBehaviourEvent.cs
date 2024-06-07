namespace COG.Listener.Event.Impl;

public class TaskPanelBehaviourEvent : Event
{
    public TaskPanelBehaviour Behaviour { get; }

    public TaskPanelBehaviourEvent(TaskPanelBehaviour behaviour)
    {
        Behaviour = behaviour;
    }
}