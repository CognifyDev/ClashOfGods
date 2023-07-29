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