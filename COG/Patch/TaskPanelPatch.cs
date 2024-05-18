using COG.Listener;
using COG.Listener.Event.Impl.TPBehaviour;

namespace COG.Patch;

[HarmonyPatch(typeof(TaskPanelBehaviour), nameof(TaskPanelBehaviour.SetTaskText))]
public static class TaskPanelPatch
{
    public static bool Prefix(TaskPanelBehaviour behaviour, ref string str)
    {
        var @event = new TaskPanelBehaviourSetTaskTextEvent(behaviour);
        bool result = ListenerManager.GetManager().ExecuteHandlers(@event, EventHandlerType.Prefix);
        
        str = @event.GetTaskString();
        return result;
    }

    public static void Postfix(TaskPanelBehaviour behaviour, [HarmonyArgument(0)] ref string str)
    {
        var @event = new TaskPanelBehaviourSetTaskTextEvent(behaviour);

        ListenerManager.GetManager().ExecuteHandlers(@event, EventHandlerType.Postfix);
        str = @event.GetTaskString();
    }
}