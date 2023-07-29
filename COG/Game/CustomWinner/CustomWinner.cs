using COG.Role;

namespace COG.Game.CustomWinner;

public class CustomWinner
{
    public static readonly CustomWinner Empty = new(CampType.Unknown);

    public CampType CampType { get; }
    
    public CustomWinner(CampType campType)
    {
        CampType = campType;
    }

    public bool CanWin { get; private set; }
    public bool Peaceful { get; private set; }

    public void SetWin(bool peaceful)
    {
        CanWin = true;
        Peaceful = peaceful;
    }
}