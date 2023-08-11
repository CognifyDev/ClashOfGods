using COG.Config.Impl;
using COG.Role;
using UnityEngine;

namespace COG.Utils;

public static class CampUtils
{
    public static Color GetColor(this CampType campType)
    {
        return campType switch
        {
            CampType.Crewmate => Color.white,
            CampType.Unknown => Color.white,
            CampType.Impostor => Color.red,
            CampType.Neutral => Palette.AcceptedGreen,
            _ => Color.white
        };
    }

    public static string GetDescription(this CampType campType)
    {
        return campType switch
        {
            CampType.Crewmate => LanguageConfig.Instance.CrewmateCampDescription,
            CampType.Unknown => LanguageConfig.Instance.UnknownCampDescription,
            CampType.Impostor => LanguageConfig.Instance.ImpostorCampDescription,
            CampType.Neutral => LanguageConfig.Instance.NeutralCampDescription,
            _ => LanguageConfig.Instance.UnknownCampDescription
        };
    }

    public static string GetName(this CampType campType)
    {
        return campType switch
        {
            CampType.Crewmate => LanguageConfig.Instance.CrewmateCamp,
            CampType.Unknown => LanguageConfig.Instance.UnknownCamp,
            CampType.Impostor => LanguageConfig.Instance.ImpostorCamp,
            CampType.Neutral => LanguageConfig.Instance.NeutralCamp,
            _ => LanguageConfig.Instance.UnknownCamp
        };
    }
}