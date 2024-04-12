using System;
using System.Threading;
using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Constant;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.Utils;
using COG.Utils.Coding;
using UnityEngine;

namespace COG.Role.Impl.Impostor;

[Unfinished]
[NotTested]
// ReSharper disable All
public class Troublemaker : Role
{
    private CustomOption? MakeTroubleCd { get; }
    private CustomOption? MakeTroubleDuration { get; }

    private CustomButton MakeTroubleButton { get; }

    // private static bool _makeTrouble = false;

    private static Thread? _task;

    public Troublemaker() : base(LanguageConfig.Instance.TroublemakerName, Palette.ImpostorRed, CampType.Impostor, true)
    {
        long? startTime;
        CanKill = true;
        CanVent = true;
        CanSabotage = true;
        BaseRoleType = RoleTypes.Impostor;
        Description = LanguageConfig.Instance.TroublemakerDescription;
        MakeTroubleCd = CustomOption.Create(CustomOption.OptionPageType.Impostor,
            LanguageConfig.Instance.TroublemakerCooldown, 15f, 11f, 120f, 1f, MainRoleOption);
        MakeTroubleDuration = CustomOption.Create(CustomOption.OptionPageType.Impostor,
            LanguageConfig.Instance.TroublemakerDuration, 10f, 1f, 10f, 1f, MainRoleOption);
        MakeTroubleButton = CustomButton.Create(
            () =>
            {

                if (_task != null)
                {
                    startTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                }
                _task = new Thread(() =>
                {
                    while (true)
                    {
                        startTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                        var now = DateTimeOffset.Now.ToUnixTimeSeconds();
                        if (MakeTroubleDuration != null && now - MakeTroubleDuration.GetFloat() >= startTime)
                        {
                            return;
                        }
                    }
                });
                _task.Start();

            },
            () => MakeTroubleButton?.ResetCooldown(),
            couldUse: () => true,
            () => true,
            ResourceUtils.LoadSpriteFromResources(ResourcesConstant.MakeTroubleButton, 100f)!,
            row: 2,
            KeyCode.C,
            LanguageConfig.Instance.MakeTrouble,
            (Cooldown)MakeTroubleCd!.GetFloat,
            0
        );

        AddButton(MakeTroubleButton);
    }
}