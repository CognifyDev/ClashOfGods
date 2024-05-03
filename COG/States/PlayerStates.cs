namespace COG.States;

public static class PlayerStates
{
    public static bool IsShowingMap()
    {
        var mapBehaviour = MapBehaviour.Instance;
        return mapBehaviour != null && mapBehaviour.IsOpen;
    }
}