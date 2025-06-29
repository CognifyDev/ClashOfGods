using COG.Config.Impl;
using COG.Listener;
using COG.Listener.Event.Impl.Game;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.SubRole;

public class SpeedBooster : CustomRole, IListener
{
    private CustomOption IncreasingSpeed { get; }
    
    public SpeedBooster() : base(Color.cyan)
    {
        IncreasingSpeed = CreateOption(() => LanguageConfig.Instance.SpeedBoosterIncreasingSpeed,
            new FloatOptionValueRule(0.5F, 0.5F, 10F, 1F, NumberSuffixes.Multiplier));
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnGameStart(GameStartEvent _)
    {
        var player = PlayerControl.LocalPlayer;
        if (!player.IsRole(this))
        {
            return;
        }

        player.MyPhysics.body.velocity *= IncreasingSpeed.GetFloat();
    }

    public override IListener GetListener()
    {
        return this;
    }

    public override string GetNameInConfig()
    {
        return "speed-booster";
    }
}