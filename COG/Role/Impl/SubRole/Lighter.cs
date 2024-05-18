using COG.Config.Impl;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.States;
using UnityEngine;

namespace COG.Role.Impl.SubRole;

public class Lighter : Role, IListener
{
    public Lighter() : base(LanguageConfig.Instance.LighterName, Color.yellow, CampType.Unknown, true)
    {
        Description = LanguageConfig.Instance.LighterDescription;
        IsSubRole = true;
    }

    [EventHandler(EventHandlerType.Prefix)]
    public bool OnPlayerAdjustLighting(PlayerAdjustLightingEvent @event)
    {
        if (!GameStates.InGame) return true;
        var player = @event.Player;
        if (player == null)
        {
            return true;
        }
        
        player.SetFlashlightInputMethod();
        player.lightSource.SetupLightingForGameplay(false, 0.75f, player.TargetFlashlight.transform);
        return false;
    }

    public override IListener GetListener()
    {
        return this;
    }
}