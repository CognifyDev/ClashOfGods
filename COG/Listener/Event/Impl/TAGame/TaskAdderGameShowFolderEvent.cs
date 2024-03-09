namespace COG.Listener.Event.Impl.TAGame;

public class TaskAdderGameShowFolderEvent : TaskAdderGameEvent
{
    private TaskFolder _taskFolder;

    public TaskAdderGameShowFolderEvent(TaskAdderGame taskAdderGame, TaskFolder taskFolder) : base(taskAdderGame)
    {
        _taskFolder = taskFolder;
    }

    public void SetTaskFolder(TaskFolder taskFolder)
    {
        _taskFolder = taskFolder;
    }

    public TaskFolder GetTaskFolder()
    {
        return _taskFolder;
    }
}