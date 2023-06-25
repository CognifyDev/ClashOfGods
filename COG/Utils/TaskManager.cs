namespace COG.Utils;

public class TaskManager
{
    private static readonly List<Task> Tasks = new();

    public static void RunTaskLater(LateTask task)
    {
        Tasks.Add(task);
    }

    public static List<Task> GetTasks() => Tasks;
}