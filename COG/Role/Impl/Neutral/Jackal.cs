using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Constant;
using COG.Listener;
using COG.Listener.Event.Impl.Game;
using COG.Listener.Event.Impl.HManager;
using COG.Rpc;
using COG.States;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace COG.Role.Impl.Neutral;

public class Jackal : Role, IListener
{
    private CustomOption CreateSidekickCd { get; } = null!;
    private CustomOption JackalKillCd { get; } = null!;
    private CustomButton CreateSidekickButton { get; }
    private CustomButton JackalKillButton { get; }
    private static PlayerControl? CurrentTarget { get; set; }
    private bool CreatedSidekick { get; set; }
    public static Dictionary<PlayerControl, PlayerControl> JackalSidekick { get; set; } = new();

    public Jackal() : base(LanguageConfig.Instance.JackalName, new(0, 180f, 235f), CampType.Neutral, true)
    {
        Description = LanguageConfig.Instance.JackalDescription;
        BaseRoleType = RoleTypes.Crewmate;

        CanSabotage = true;
        CanVent = true;

        if (ShowInOptions)
        {
            CreateSidekickCd = CustomOption.Create(CustomOption.TabType.Neutral,
                LanguageConfig.Instance.JackalCreateSidekickCd, 30f, 10f, 60f, 5f, MainRoleOption);
            JackalKillCd = CustomOption.Create(CustomOption.TabType.Neutral, LanguageConfig.Instance.JackalKillCd, 30f,
                10f, 60f, 5f, MainRoleOption);
        }

        CreateSidekickButton = CustomButton.Create(
            () =>
            {
                PlayerControl.LocalPlayer.RpcCreateSidekick(CurrentTarget!);
                CreatedSidekick = true;
            },
            () => CreateSidekickButton?.ResetCooldown(),
            couldUse: () => CurrentTarget,
            () =>
            {
                if (!CustomRoleManager.GetManager().GetTypeRoleInstance<Sidekick>().SidekickCanCreateSidekick.GetBool()
                    && JackalSidekick.ContainsValue(PlayerControl.LocalPlayer)) return false;
                return !CreatedSidekick;
            },
            null!,
            2,
            KeyCode.C,
            LanguageConfig.Instance.CreateSidekick,
            () => CreateSidekickCd?.GetFloat() ?? 30f,
            0
        );

        JackalKillButton = CustomButton.Create(
            () => { PlayerControl.LocalPlayer.CmdCheckMurder(CurrentTarget); },
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
            () => JackalKillCd?.GetFloat() ?? 30f,
            -1
        );

        AddButton(CreateSidekickButton);
        AddButton(JackalKillButton);
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnHudUpdate(HudManagerUpdateEvent @event)
    {
        if (!(GameStates.InGame && PlayerControl.LocalPlayer.IsRole(this))) return;
        if (PlayerControl.LocalPlayer.IsAlive())
            CurrentTarget = PlayerControl.LocalPlayer.SetClosestPlayerOutline(Color);
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnGameStart(GameStartEvent @event)
    {
        CreatedSidekick = false;
        JackalSidekick.Clear();
    }

    public override IListener GetListener() => this;
}

public static class JackalUtils
{
    public static bool IsInJackalTeam(this PlayerControl pc) => pc.IsRole<Jackal>() || pc.IsRole<Sidekick>();

    public static void RpcCreateSidekick(this PlayerControl jackal, PlayerControl target)
    {
        if (!(jackal && target)) return;

        var writer = RpcUtils.StartRpcImmediately(jackal, KnownRpc.CreateSidekick);
        writer.Write(jackal.PlayerId);
        writer.Write(target.PlayerId);
        writer.Finish();

        CreateSidekick(jackal, target);
    }

    public static void CreateSidekick(this PlayerControl jackal, PlayerControl target)
    {
        if (!(jackal && target)) return;
        target.SetCustomRole<Sidekick>();
        Jackal.JackalSidekick.Add(jackal, target);
    }
}