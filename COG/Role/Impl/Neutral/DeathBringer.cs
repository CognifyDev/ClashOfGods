using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using COG.Config.Impl;
using COG.Constant;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.UI.Vanilla.KillButton;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.Neutral;

[SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
public class DeathBringer : CustomRole, IListener
{
    private const string PlayerStaredAtTag = "staredAt_DeathBringer";

    private readonly CustomButton _stareButton;

    private readonly CustomOption _killCooldown, _neededPlayerNumber;

    private readonly List<PlayerControl> _staredPlayers = new();

    private PlayerControl? _target;
    
    public DeathBringer() : base(new Color(112, 48, 160, 100), CampType.Neutral)
    {
        CanKill = true;

        _killCooldown = CreateOption(() => LanguageConfig.Instance.KillCooldown,
            new FloatOptionValueRule(1F, 1F, 60F, 30F, NumberSuffixes.Seconds));
        _neededPlayerNumber = CreateOption(() => LanguageConfig.Instance.DeathBringerNeededPlayerNumber,
            new FloatOptionValueRule(1F, 1F, 15F, 5F));
        
        _stareButton = CustomButton.Of(
            "death-bringer-stare",
            () =>
            {
                _staredPlayers.Add(_target!);
                KillButtonManager.ResetCooldown();
            },
            () => _stareButton!.ResetCooldown(),
            () => PlayerControl.LocalPlayer.CheckClosestTargetInKillDistance(out _target),
            () => true,
            ResourceUtils.LoadSprite(ResourcesConstant.StareButton)!,
            3,
            KeyCode.R,
            LanguageConfig.Instance.StareAction,
            () => _killCooldown.GetFloat(),
            -1);

        KillButtonSetting.ForceShow = () => PlayerUtils.GetAllAlivePlayers().Count <= _neededPlayerNumber.GetFloat();
        KillButtonSetting.CustomCooldown = _killCooldown.GetFloat;

        AddButton(_stareButton);
    }

    public override IListener GetListener()
    {
        return this;
    }

    [EventHandler(EventHandlerType.Postfix)]
    [OnlyLocalPlayerWithThisRoleInvokable]
    public void OnPlayerReport(PlayerReportDeadBodyEvent @event)
    {
        foreach (var target in _staredPlayers)
            target.CmdCheckMurder(target);
    }

    public override string GetNameInConfig()
    {
        return "death-bringer";
    }
}