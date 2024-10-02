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
    private readonly CustomButton _killButton;
    private bool _hasGiven;

    private int _killTimes = 1;

    private readonly CustomOption _minCrewmateNumber;

    public Vigilante() : base(ColorUtils.AsColor("#ffcc00"), CampType.Crewmate)
    {
        CanKill = false;
        CanVent = false;

        _killButton = CustomButton.Create(
            () =>
            {
                var target = PlayerControl.LocalPlayer.GetClosestPlayer();
                if (target == null) return;
                PlayerControl.LocalPlayer.RpcMurderPlayer(target, true);
                _killTimes--;
                _killButton!.UsesRemaining = _killTimes;
            },
            () => _killButton!.ResetCooldown(),
            () => _killButton!.HasButton() &&
                  PlayerControl.LocalPlayer.GetClosestPlayer(true, GameUtils.GetGameOptions().KillDistance),
            () => true,
            ResourceUtils.LoadSprite(ResourcesConstant.GeneralKillButton)!,
            2,
            KeyCode.Q,
            LanguageConfig.Instance.KillAction,
            () => 1f,
            -1);

        AddButton(_killButton);
        
        _minCrewmateNumber = CreateOption(() => LanguageConfig.Instance.VigilanteMinCrewmateNumber,
            new IntOptionValueRule(1, 1, 15, 3));
    }

    public override void ClearRoleGameData()
    {
        _killTimes = 1;
        _hasGiven = false;
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnPlayerFixedUpdate(PlayerFixedUpdateEvent @event)
    {
        var crewmates
            = PlayerUtils.GetAllAlivePlayers().Where(p => p.GetMainRole().CampType == CampType.Crewmate);
        if (crewmates.Count() > _minCrewmateNumber.GetInt() || _hasGiven) return;
        _killTimes++;
        _hasGiven = true;
    }

    public override IListener GetListener()
    {
        return this;
    }
}