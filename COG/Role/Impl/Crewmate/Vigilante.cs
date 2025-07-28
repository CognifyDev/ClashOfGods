using System.Linq;
using COG.Config.Impl;
using COG.Constant;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

public class Vigilante : CustomRole, IListener
{
    private bool _hasGiven;

    private readonly CustomOption _minCrewmateNumber;

    public Vigilante() : base(ColorUtils.AsColor("#ffcc00"), CampType.Crewmate)
    {
        CanKill = true;
        CanVent = false;

        DefaultKillButtonSetting.UsesLimit = int.MaxValue;
        DefaultKillButtonSetting.RemainingUses = 1;
        
        _minCrewmateNumber = CreateOption(() => LanguageConfig.Instance.VigilanteMinCrewmateNumber,
            new FloatOptionValueRule(1F, 1F, 15F, 3F));
    }

    public override void ClearRoleGameData()
    {
        DefaultKillButtonSetting.RemainingUses = 1;
        _hasGiven = false;
    }

    [EventHandler(EventHandlerType.Postfix)]
    [OnlyLocalPlayerWithThisRoleInvokable]
    public void OnPlayerFixedUpdate(PlayerFixedUpdateEvent @event)
    {
        if (PlayerUtils.AllCrewmates.Count() > _minCrewmateNumber.GetInt() || _hasGiven) return;

        DefaultKillButtonSetting.RemainingUses++;
        _hasGiven = true;
    }

    public override IListener GetListener()
    {
        return this;
    }
}