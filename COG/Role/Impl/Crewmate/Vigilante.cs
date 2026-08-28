using System.Linq;
using COG.Config.Impl;
using COG.Listener;
using COG.Listener.Attribute;
using COG.Listener.Event.Impl.Player;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;

namespace COG.Role.Impl.Crewmate;

public class Vigilante : COG.Role.Camp.CrewmateRole
{
    private readonly CustomOption _minCrewmateNumber;
    private bool _hasGiven;

    public Vigilante()
    {
        CanKill = true;

        DefaultKillButtonSetting.UsesLimit = int.MaxValue;
        DefaultKillButtonSetting.RemainingUses = 1;

        _minCrewmateNumber = CreateOption(() => LanguageConfig.Instance.GetString("role.crewmate.vigilante.min-crewmate-number"),
            new FloatOptionValueRule(1, 1, 15, 3));

        On<PlayerFixedUpdateEvent>(OnPlayerFixedUpdate);
    }

    public override void ClearRoleGameData()
    {
        DefaultKillButtonSetting.RemainingUses = 1;
        _hasGiven = false;
    }

    private void OnPlayerFixedUpdate(PlayerFixedUpdateEvent @event)
    {
        if (!GameStates.InRealGame) return;
        if (PlayerUtils.AllCrewmates.Count() > _minCrewmateNumber.GetInt() || _hasGiven) return;

        DefaultKillButtonSetting.RemainingUses++;
        _hasGiven = true;
    }
}