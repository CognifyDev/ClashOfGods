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
            CampType.Crewmate => LanguageConfig.Instance.GetString("camp.crewmate.description"),
            CampType.Unknown => LanguageConfig.Instance.GetString("camp.unknown.description"),
            CampType.Impostor => LanguageConfig.Instance.GetString("camp.impostor.description"),
            CampType.Neutral => LanguageConfig.Instance.GetString("camp.neutral.description"),
            _ => LanguageConfig.Instance.GetString("camp.unknown.description")
        };
    }

    public static string GetName(this CampType campType)
    {
        return campType switch
        {
            CampType.Crewmate => LanguageConfig.Instance.GetString("camp.crewmate.name"),
            CampType.Unknown => LanguageConfig.Instance.GetString("camp.unknown.name"),
            CampType.Impostor => LanguageConfig.Instance.GetString("camp.impostor.name"),
            CampType.Neutral => LanguageConfig.Instance.GetString("camp.neutral.name"),
            _ => LanguageConfig.Instance.GetString("camp.unknown.name")
        };
    }
}