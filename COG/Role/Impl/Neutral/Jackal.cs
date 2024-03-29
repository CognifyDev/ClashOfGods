using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Constant;
using COG.Listener;
using COG.Listener.Event.Impl.HManager;
using COG.Listener.Event.Impl.Player;
using COG.Role;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.Utils;
using COG.Utils.Coding;
using UnityEngine;

namespace COG.Role.Impl.Neutral;

[NotUsed]
[NotTested]
public class Jackal : Role, IListener
{
    private CustomOption SidekickCanCreateSidekick { get; }
    private CustomOption CreateSidekickCd { get; }
    private CustomOption JackalKillCd { get; }
    private CustomButton CreateSidekickButton { get; }
    private CustomButton JackalKillButton { get; }
    private static PlayerControl? CurrentTarget { get; set; }
    public Jackal() : base("Jackal", new(0, 180f, 235f), CampType.Neutral, true)
    {
        Description = "";
        BaseRoleType = RoleTypes.Crewmate;

        if (ShowInOptions)
        {
            SidekickCanCreateSidekick = CustomOption.Create(false, CustomOption.CustomOptionType.Neutral, "", true, MainRoleOption);
            CreateSidekickCd = CustomOption.Create(false, CustomOption.CustomOptionType.Neutral, "", 30f, 10f, 60f, 5f, MainRoleOption)!;
            JackalKillCd = CustomOption.Create(false, CustomOption.CustomOptionType.Neutral, "", 30f, 10f, 60f, 5f, MainRoleOption)!;
        }

        CreateSidekickButton = CustomButton.Create(
            () =>
            {
                CurrentTarget!.RpcSetCustomRole<Sidekick>();
            },
            () => CreateSidekickButton?.ResetCooldown(),
            couldUse: () => CurrentTarget,
            () => true,
            null!,
            2,
            KeyCode.C,
            "",
            (Cooldown)CreateSidekickCd!.GetFloat,
            0
        );

        JackalKillButton= CustomButton.Create(
            () =>
            {
                PlayerControl.LocalPlayer.CmdCheckMurder(CurrentTarget);
            },
            () => JackalKillButton?.ResetCooldown(),
            couldUse: () =>
            {
                var target = CurrentTarget;
                if (target == null) return false;
                var localPlayer = PlayerControl.LocalPlayer;
                var localLocation = localPlayer.GetTruePosition();
                var targetLocation = target.GetTruePosition();
                var distance = Vector2.Distance(localLocation, targetLocation);
                return GameUtils.GetGameOptions().KillDistance >= distance;
            },
            () => true,
            ResourceUtils.LoadSpriteFromResources(ResourcesConstant.GeneralKillButton, 100f)!,
            row: 1,
            KeyCode.Q,
            LanguageConfig.Instance.KillAction,
            (Cooldown)(JackalKillCd?.GetFloat() ?? 30f),
            -1
        );

        AddButton(CreateSidekickButton);
        AddButton(JackalKillButton);
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnHudUpdate(HudManagerUpdateEvent @event)
    {
        if (!PlayerControl.LocalPlayer.IsRole(this)) return;
        CurrentTarget = PlayerControl.LocalPlayer.SetClosestPlayerOutline(Color);
    }

    public override IListener GetListener() => this;
}