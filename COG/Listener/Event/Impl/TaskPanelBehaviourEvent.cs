namespace COG.Listener.Event.Impl;

public class TaskPanelBehaviourEvent : Event
{
    public TaskPanelBehaviourEvent(TaskPanelBehaviour behaviour)
    {
        Behaviour = behaviour;
    }

    public TaskPanelBehaviour Behaviour { get; }
}