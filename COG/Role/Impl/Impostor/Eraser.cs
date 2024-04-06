using AmongUs.GameOptions;
using COG.Listener;
using COG.Role.Impl.Crewmate;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.Utils;
using COG.Utils.Coding;
using System.Collections.Generic;
using UnityEngine;

namespace COG.Role.Impl.Impostor;

[NotUsed]
[Unfinished]
public class Eraser : Role, IListener
{
    public CustomOption InitialEraseCooldown { get; }
    public CustomOption IncreaseCooldownAfterErasing { get; }
    public CustomButton EraseButton { get; }
    public static PlayerControl? CurrentTarget { get; set; }
    public Eraser() : base("", Palette.ImpostorRed, CampType.Impostor, true)
    {
        if (ShowInOptions)
        {
            var type = ToCustomOption(this);
            InitialEraseCooldown = CustomOption.Create(false, type, "", 30f, 10f, 60f, 5f, MainRoleOption);
            IncreaseCooldownAfterErasing = CustomOption.Create(false, type, "", 10f, 5f, 15f, 5f, MainRoleOption);

        }

        EraseButton = CustomButton.Create(() =>
        {
            CurrentTarget!.RpcSetCustomRole<Crewmate.Crewmate>();
            var currentCd = EraseButton!.Cooldown();
            EraseButton.SetCooldown(currentCd + (IncreaseCooldownAfterErasing?.GetFloat() ?? 10f));
        },
        EraseButton!.ResetCooldown,
        couldUse: () =>
        {
            return !CurrentTarget;
        },
        () => true,
        null!,
        2,
        KeyCode.E,
        "",
        (Cooldown)(() => InitialEraseCooldown?.GetFloat() ?? 30f),
        -1);
    }

    public override IListener GetListener() => this;
}