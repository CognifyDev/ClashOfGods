using COG.Config.Impl;
using COG.Listener;
using UnityEngine;

namespace COG.Role.Impl.Impostor;

public class Impostor : Role
{
    public Impostor() : base(LanguageConfig.Instance.ImpostorName, Color.red, CampType.Impostor)
    {
        Description = LanguageConfig.Instance.ImpostorDescription;
    }

    public override IListener GetListener(PlayerControl player)
    {
        return IListener.Empty;
    }
}