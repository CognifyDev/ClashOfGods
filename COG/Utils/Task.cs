namespace COG.Utils;

public abstract class Task
{
    public string Name { get; }

    public Task(string name = "Unknown")
    {
        Name = name;
    }

    public abstract void Run();
}

public abstract class LateTask : Task
{
    public float Time { get; }

    public LateTask(float time, string name = "Unknown") : base(name)
    {
        Time = SystemUtils.GetTimeStamp() + time;
    }
}