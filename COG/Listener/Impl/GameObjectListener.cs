namespace COG.Listener.Impl;

public class GameObjectListener : IListener
{
    public bool OnDeadBodyClick(DeadBody deadBody)
    {
        // Main.Logger.LogInfo(deadBody.TruePosition.ToString());
        return true;
    }
}