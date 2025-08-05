using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Listener;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using System;
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

public class Sheriff : CustomRole, IListener
{
    public const string MisfireMurderMessage = "sheriff_misfire";

    public Sheriff() : base(Color.yellow, CampType.Crewmate)
    {
        CanKill = true;

        BaseRoleType = RoleTypes.Crewmate; 
        
        SheriffKillCd = CreateOption(() => LanguageConfig.Instance.KillCooldown,
                new FloatOptionValueRule(10f, 5f, 60f, 30f, NumberSuffixes.Seconds));

        DefaultKillButtonSetting.CustomCooldown = SheriffKillCd.GetFloat;
        DefaultKillButtonSetting.SetBeforeMurder(button =>
        {
            var localData = PlayerControl.LocalPlayer.Data;
            var current = button.currentTarget;

            if (current!.GetMainRole().CampType != CampType.Crewmate)
            {
                return current;
            }
            else
            {
                DefaultKillButtonSetting.ExtraRpcMessage = $"{MisfireMurderMessage}{current.PlayerId}";
                return PlayerControl.LocalPlayer;
            }
        });

        DefaultKillButtonSetting.AddAfterClick(() => DefaultKillButtonSetting.ExtraRpcMessage = null);
    }

    private CustomOption SheriffKillCd { get; }
}