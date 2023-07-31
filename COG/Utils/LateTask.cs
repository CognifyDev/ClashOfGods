using System;

namespace COG.Utils;

public class LateTask
{
    private Action _action;
    private float _time;
    
    public LateTask(Action action, float time)
    {
        _action = action;
        _time = time;
    }
}