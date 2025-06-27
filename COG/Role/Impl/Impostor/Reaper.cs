using System.Linq;
using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Constant;
using COG.Listener;
using COG.Listener.Event.Impl.Game;
using COG.Listener.Event.Impl.Player;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using KeyCode = UnityEngine.KeyCode;


namespace COG.Role.Impl.Impostor;

public class Reaper : CustomRole, IListener
{
    private CustomOption TimeToReduce { get; }
    
    private CustomButton KillButton { get; }

    private float _cooldown;
    
    public Reaper() : base(Palette.ImpostorRed, CampType.Impostor)
    {
        BaseRoleType = RoleTypes.Impostor;
        CanKill = true;
        CanVent = true;
        CanSabotage = true;
        
        TimeToReduce = CreateOption(() => LanguageConfig.Instance.ReaperTimeToReduce,
            new FloatOptionValueRule(1F, 0.5F, 15F, 1.5F, NumberSuffixes.Seconds));

        KillButtonSetting.CustomCooldown = () => _cooldown;
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnPlayerMurder(PlayerMurderEvent @event)
    {
        var target = @event.Target;
        if (target.PlayerId == PlayerControl.LocalPlayer.PlayerId)
        {
            return;
        }

        if (_cooldown > 1)
        {
            _cooldown = TimeToReduce.GetFloat();
        }
    }

    public void OnGameStart(GameStartEvent @event)
    {
        ClearRoleGameData();
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