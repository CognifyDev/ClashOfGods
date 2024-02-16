using System.Threading;
using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Listener;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.Impostor;

/*
 * TODO
 * 完成 干扰
 */
public class Troublemaker : Role
{
    private CustomOption? MakeTroubleCd { get; }
    private CustomOption? MakeTroubleDuration { get; }
    
    private CustomButton MakeTroubleButton { get; }

    private static bool _makeTrouble = false;

    private static Thread? _task;
    private static long? _startTime;
    
    public Troublemaker() : base(LanguageConfig.Instance.TroublemakerName, Palette.ImpostorRed, CampType.Impostor, true)
    {
        CanKill = true;
        CanVent = true;
        CanSabotage = true;
        BaseRoleType = RoleTypes.Impostor;
        Description = LanguageConfig.Instance.TroublemakerDescription;
        MakeTroubleCd = CustomOption.Create(false, CustomOption.CustomOptionType.Impostor, 
            LanguageConfig.Instance.TroublemakerCooldown, 15f, 11f, 120f, 1f, MainRoleOption);
        MakeTroubleDuration = CustomOption.Create(false, CustomOption.CustomOptionType.Impostor, 
            LanguageConfig.Instance.TroublemakerDuration, 10f, 1f, 10f, 1f, MainRoleOption);
        MakeTroubleButton = CustomButton.Create(
            () =>
            {
                /*
                if (_task != null)
                {
                    _startTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                }
                _task = new Thread(() =>
                {
                    while (true)
                    {
                        _startTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                        var now = DateTimeOffset.Now.ToUnixTimeSeconds();
                        if (now - MakeTroubleDuration.GetFloat() >= _startTime)
                        {
                            return;
                        }
                    }
                });
                _task.Start();
                */
            },
            () => MakeTroubleButton?.ResetCooldown(),
            couldUse: () => true,
            () => true,
            ResourceUtils.LoadSpriteFromResources("COG.Resources.InDLL.Images.Buttons.MakeTrouble.png", 100f)!,
            row: 2,
            KeyCode.C,
            LanguageConfig.Instance.MakeTrouble,
            (Cooldown)MakeTroubleCd.GetFloat,
            0
        );
        
        AddButton(MakeTroubleButton);
    }

    public override IListener GetListener(PlayerControl player) => IListener.Empty;
}