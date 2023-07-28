using COG.Config.Impl;
using COG.Listener;
using UnityEngine;

namespace COG.Role.Impl;

public class Unknown : Role
{
    public Unknown() : base(LanguageConfig.Instance.UnknownName, Color.white, CampType.Unknown, false)
    {
        Description = LanguageConfig.Instance.UnknownDescription;
    }

    public override IListener GetListener(PlayerControl player)
    {
        return IListener.Empty;
    }
}