namespace COG.Utils;

public class Task
{
    public string Name { get; }
    public Action Action { get; }

    public Task(Action action, string name = "Unknown")
    {
        Name = name;
        Action = action;
    }
}

public class LateTask : Task
{
    public float Time { get; }

    public LateTask(Action action, float time, string name = "Unknown") : base(action, name)
    {
        Time = SystemUtils.GetTimeStamp() + time;
    }
}