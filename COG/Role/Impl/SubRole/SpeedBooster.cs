using COG.Config.Impl;
using COG.Listener.Event.Impl;
using COG.Listener.Event.Impl.Game;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using UnityEngine;

namespace COG.Role.Impl.SubRole;

public class SpeedBooster : CustomRole
{
    public SpeedBooster() : base(Color.cyan)
    {
        IncreasingSpeed = CreateOption(() => LanguageConfig.Instance.GetString("role.sub-roles.speed-booster.increasing-speed"),
            new FloatOptionValueRule(0.5F, 0.5F, 10F, 1F, NumberSuffixes.Multiplier));

        On<GameStartEvent>(OnGameStart);
    }

    private CustomOption IncreasingSpeed { get; }

    private void OnGameStart(GameStartEvent @event)
    {
        var player = PlayerControl.LocalPlayer;
        if (!player.Data.IsDead && GameStates.InRealGame) player.MyPhysics.Speed *= IncreasingSpeed.GetFloat();
    }

    public override string GetNameInConfig()
    {
        return "speed-booster";
    }
}