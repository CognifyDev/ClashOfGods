using System.Linq;
using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Constant;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.Rpc;
using COG.States;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.Impostor;

public class Cleaner : CustomRole, IListener
{
    public Cleaner() : base(LanguageConfig.Instance.CleanerName, Palette.ImpostorRed, CampType.Impostor)
    {
        Description = LanguageConfig.Instance.CleanerDescription;
        BaseRoleType = RoleTypes.Impostor;
        CanKill = true;

        if (ShowInOptions)
            CleanBodyCd = CustomOption.Create(CustomOption.TabType.Impostor,
                LanguageConfig.Instance.CleanBodyCooldown, 30f, 1f, 60f, 1f, MainRoleOption);

        CleanBodyButton = CustomButton.Create(
            () =>
            {
                var body = PlayerUtils.GetClosestBody();
                if (!body) return;
                RpcCleanDeadBody(body!);
                KillButton?.ResetCooldown();
            },
            () => CleanBodyButton?.ResetCooldown(),
            () => PlayerUtils.GetClosestBody() != null,
            () => true,
            ResourceUtils.LoadSpriteFromResources(ResourcesConstant.CleanDeadBodyButton, 100f)!,
            2,
            KeyCode.C,
            LanguageConfig.Instance.CleanAction,
            () => CleanBodyCd?.GetFloat() ?? 30f,
            0
        );

        KillButton = CustomButton.Create(
            () =>
            {
                PlayerControl.LocalPlayer.CmdCheckMurder(ClosestTarget);
                CleanBodyButton.ResetCooldown();
            },
            () => KillButton?.ResetCooldown(),
            () =>
            {
                var target = ClosestTarget = PlayerControl.LocalPlayer;
                if (target == null) return false;
                var localPlayer = PlayerControl.LocalPlayer;
                var localLocation = localPlayer.GetTruePosition();
                var targetLocation = target.GetTruePosition();
                var distance = Vector2.Distance(localLocation, targetLocation);
                return GameUtils.GetGameOptions().KillDistance >= distance;
            },
            () => true,
            ResourceUtils.LoadSpriteFromResources(ResourcesConstant.GeneralKillButton, 100f)!,
            1,
            KeyCode.Q,
            LanguageConfig.Instance.KillAction,
            () => CleanBodyCd?.GetFloat() ?? 30f,
            -1
        );

        AddButton(CleanBodyButton);
    }

    private CustomOption? CleanBodyCd { get; }
    private CustomButton CleanBodyButton { get; }
    private CustomButton KillButton { get; }
    private static PlayerControl? ClosestTarget { get; set; }

    public void RpcCleanDeadBody(DeadBody body)
    {
        var writer = RpcUtils.StartRpcImmediately(PlayerControl.LocalPlayer, KnownRpc.CleanDeadBody);
        writer.Write(body.ParentId);
        writer.Finish();
        CleanDeadBody(body);
    }

    public void CleanDeadBody(DeadBody body)
    {
        body.gameObject.SetActive(false);
        // idk why it make PlayerControl.FixedUpdate() throw System.NullReferenceException when i destroy the body
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnRPCReceived(PlayerHandleRpcEvent @event)
    {
        if (!GameStates.InGame) return;
        var callId = @event.CallId;
        var reader = @event.MessageReader;
        if (callId != (byte)KnownRpc.CleanDeadBody) return;
        var pid = reader.ReadByte();
        var body = Object.FindObjectsOfType<DeadBody>().ToList().FirstOrDefault(b => b.ParentId == pid);
        if (!body) return;
        CleanDeadBody(body!);
    }

    public override IListener GetListener()
    {
        return this;
    }
}