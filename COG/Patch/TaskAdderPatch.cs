using COG.Listener;
using COG.Listener.Event.Impl.TAButton;
using COG.Listener.Event.Impl.TAGame;
using COG.NewListener;

namespace COG.Patch;

[HarmonyPatch(typeof(TaskAdderGame), nameof(TaskAdderGame.ShowFolder))]
public static class TaskAdderPatch
{
    public static bool Prefix(TaskAdderGame __instance, [HarmonyArgument(0)] TaskFolder taskFolder)
    {
        return ListenerManager.GetManager().ExecuteHandlers(new TaskAdderGameShowFolderEvent(__instance, taskFolder),
            EventHandlerType.Prefix);
    }

    public static void Postfix(TaskAdderGame __instance, [HarmonyArgument(0)] TaskFolder taskFolder)
    {
        ListenerManager.GetManager().ExecuteHandlers(new TaskAdderGameShowFolderEvent(__instance, taskFolder),
            EventHandlerType.Postfix);
    }
}

[HarmonyPatch(typeof(TaskAddButton))]
public static class TaskAddButtonPatch
{
    [HarmonyPatch(nameof(TaskAddButton.Update))]
    [HarmonyPrefix]
    public static bool ButtonUpdatePatch(TaskAddButton __instance)
    {
        return ListenerManager.GetManager()
            .ExecuteHandlers(new TaskAddButtonUpdateEvent(__instance), EventHandlerType.Prefix);
    }

    [HarmonyPatch(nameof(TaskAddButton.AddTask))]
    [HarmonyPrefix]
    public static bool AddTaskPatch(TaskAddButton __instance)
    {
        return ListenerManager.GetManager()
            .ExecuteHandlers(new TaskAddButtonAddTaskEvent(__instance), EventHandlerType.Prefix);
    }
}