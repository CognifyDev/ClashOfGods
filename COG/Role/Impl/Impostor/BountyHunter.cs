using COG.Config.Impl;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.Utils;
using UnityEngine;
using COG.States;
using COG.Listener.Event.Impl.Game;
using System.Linq;
using System;
using AmongUs.GameOptions;
using COG.Constant;
using COG.Utils.Coding;
using Debug = System.Diagnostics.Debug;
using Object = UnityEngine.Object;

namespace COG.Role.Impl.Impostor;

[Unfinished]
[NotTested]
// ReSharper disable All
public class BountyHunter : Role, IListener
{
    private CustomButton BHunterKillButton { get; set; }
    private CustomOption? BHunterKillCd { get; set; }
    private CustomOption? BHunterRefreshTargetTime { get; init; }
    private CustomOption? HasArrowToTarget { get; set; }
    private CustomOption? CdAfterKillingTarget { get; init; }
    private CustomOption? CdAfterKillingNonTarget { get; init; }
    private PoolablePlayer? TargetPoolable { get; set; }
    private float RefreshTargetTimer { get; set; } = float.PositiveInfinity;
    private bool TimerStarted { get; set; }
    private PlayerControl? CurrentTarget { get; set; }
    private PlayerControl? ClosestTarget { get; set; }

    public BountyHunter() : base("BountyHunter", Palette.ImpostorRed, CampType.Impostor, true)
    {
        CanKill = false; // Disable vanilla kill button
        Description = "Kill target";
        BaseRoleType = RoleTypes.Impostor;

        if (ShowInOptions)
        {
            var optionType = ToCustomOption(this);
            BHunterKillCd = CustomOption.Create(false, optionType, "cd", 30f, 10f, 60f, 5f, MainRoleOption)!;
            BHunterRefreshTargetTime =
                CustomOption.Create(false, optionType, "time", 30f, 10f, 60f, 5f, MainRoleOption)!;
            HasArrowToTarget = CustomOption.Create(false, optionType, "arrow", true, MainRoleOption);
            CdAfterKillingTarget =
                CustomOption.Create(false, optionType, "correct", 60f, 10f, 60f, 5f, MainRoleOption)!;
            CdAfterKillingNonTarget =
                CustomOption.Create(false, optionType, "incorrect", 10f, 10f, 60f, 5f, MainRoleOption)!;
        }

        BHunterKillButton = CustomButton.Create(
            () =>
            {
                if (!ClosestTarget) return;
                PlayerControl.LocalPlayer.CmdCheckMurder(ClosestTarget);
            },
            () => BHunterKillButton?.ResetCooldown(),
            couldUse: () =>
            {
                var target = PlayerControl.LocalPlayer.GetClosestPlayer();
                if (target == null) return false;
                var localPlayer = PlayerControl.LocalPlayer;
                var localLocation = localPlayer.GetTruePosition();
                var targetLocation = target.GetTruePosition();
                var distance = Vector2.Distance(localLocation, targetLocation);
                return GameUtils.GetGameOptions().KillDistance >= distance;
            },
            () => true,
            ResourceUtils.LoadSpriteFromResources(ResourcesConstant.GeneralKillButton, 100f)!,
            row: 2,
            KeyCode.Q,
            LanguageConfig.Instance.KillAction,
            (Cooldown)BHunterKillCd!.GetFloat,
            -1
        );

        AddButton(BHunterKillButton);
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void AfterPlayerFixedUpdate(PlayerFixedUpdateEvent @event)
    {
        if (!GameStates.InGame || !TimerStarted) return;
        RefreshTargetTimer -= Time.deltaTime;
        if (RefreshTargetTimer <= 0f) RefreshTarget();
        ClosestTarget = PlayerControl.LocalPlayer.GetClosestPlayer();
        if (ClosestTarget) ClosestTarget!.SetOutline(Color);
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnGameStart(GameStartEvent @event)
    {
        if (!PlayerControl.LocalPlayer.IsRole(this)) return;
        CreatePoolable();
        RefreshTarget();
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnGameEnd(GameSetEverythingUpEvent @event)
    {
        CurrentTarget = null;
        TimerStarted = false;
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnPlayerMurder(PlayerMurderEvent @event)
    {
        var victim = @event.Target;
        if (!PlayerControl.LocalPlayer.IsRole(this))
        {
            if (!CurrentTarget) return;
            if (victim.IsSamePlayer(CurrentTarget!)) RefreshTarget();
            return;
        }

        if (!Players.Any(pc => pc.IsSamePlayer(@event.Player)) || !CurrentTarget) return;
        Debug.Assert(CdAfterKillingTarget != null, nameof(CdAfterKillingTarget) + " != null");
        Debug.Assert(CdAfterKillingNonTarget != null, nameof(CdAfterKillingNonTarget) + " != null");
        BHunterKillButton.SetCooldown(victim.IsSamePlayer(CurrentTarget!)
            ? CdAfterKillingTarget.GetFloat()
            : CdAfterKillingNonTarget.GetFloat());
        RefreshTarget();
    }

    private void CreatePoolable()
    {
        TargetPoolable = Object.Instantiate(PlayerUtils.PoolablePlayerPrefab!, HudManager.Instance.transform);
        var transform = TargetPoolable.transform;
        transform.localPosition = new Vector3(-3f, -2f, 0);
        transform.localScale = new Vector3(0.7f, 0.7f, 0);
        TargetPoolable.gameObject.SetActive(true);
    }

    private void RefreshTarget()
    {
        Debug.Assert(BHunterRefreshTargetTime != null, nameof(BHunterRefreshTargetTime) + " != null");
        RefreshTargetTimer = BHunterRefreshTargetTime.GetFloat();
        TimerStarted = true;

        var selectableTargets = PlayerControl.AllPlayerControls.ToArray().Where(p =>
            p.GetRoleInstance()!.CampType != CampType.Impostor && p.IsAlive() &&
            PlayerControl.LocalPlayer.IsSamePlayer(p)).ToList();
        var r = new System.Random(DateTime.Now.Millisecond);

        if (selectableTargets.Count == 0)
        {
            Main.Logger.LogError("[Bounty Hunter] Couldn't select a new target.");
            GameUtils.SendGameMessage("");
            Debug.Assert(TargetPoolable != null, nameof(TargetPoolable) + " != null");
            TargetPoolable.gameObject.SetActive(false);
            TimerStarted = false;
            return;
        }

        CurrentTarget = selectableTargets[r.Next(0, selectableTargets.Count)];

        Debug.Assert(TargetPoolable != null, nameof(TargetPoolable) + " != null");
        CurrentTarget.SetPlayerAppearance(TargetPoolable);
    }

    public override IListener GetListener() => this;
}