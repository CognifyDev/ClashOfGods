using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using UnityEngine;


namespace COG.Role.Impl.Impostor;

public class Reaper : CustomRole, IListener
{
    private CustomOption TimeToReduce { get; }

    private float _cooldown;
    
    public Reaper() : base(Palette.ImpostorRed, CampType.Impostor)
    {
        BaseRoleType = RoleTypes.Impostor;
        CanKill = true;
        CanVent = true;
        CanSabotage = true;
        
        TimeToReduce = CreateOption(() => LanguageConfig.Instance.ReaperTimeToReduce,
            new FloatOptionValueRule(1F, 0.5F, 5F, 1.5F, NumberSuffixes.Seconds));

        DefaultKillButtonSetting.CustomCooldown = () => _cooldown;
    }

    [EventHandler(EventHandlerType.Postfix)]
    [OnlyLocalPlayerWithThisRoleInvokable]
    public void OnPlayerMurder(PlayerMurderEvent @event)
    {
        _cooldown = Mathf.Clamp(_cooldown -= TimeToReduce.GetFloat(), 1f, float.MaxValue);
    }

    public override void ClearRoleGameData()
    {
        _cooldown = GameUtils.GetGameOptions().KillCooldown;
    }

    public override IListener GetListener()
    {
        return this;
    }
}