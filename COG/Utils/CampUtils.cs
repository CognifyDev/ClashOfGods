using COG.Role;

namespace COG.Utils;

public static class CampUtils
{
    public static string GetCampString(this CampType camp)
    {
        string campString = "Unknown";
        switch (camp)
        {
            case CampType.Crewmate:
                campString = "船员";
                break; 
            case CampType.Impostor: 
                campString = "伪装者"; 
                break;
            case CampType.Neutral: 
                campString = "中立"; 
                break;
        }

        return campString;
    }
}