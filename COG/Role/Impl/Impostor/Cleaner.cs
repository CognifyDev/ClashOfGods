using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Rpc;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.Utils;
using System.Linq;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
using UnityEngine;
using Object = UnityEngine.Object;

namespace COG.Role.Impl.Impostor;

public class Cleaner : Role, IListener
{
    private CustomOption? CleanBodyCd { get; }
    private CustomButton CleanBodyButton { get; }
    
    public Cleaner() : base(LanguageConfig.Instance.CleanerName, Palette.ImpostorRed, CampType.Impostor, true)
    {
        Description = LanguageConfig.Instance.CleanerDescription;
        BaseRoleType = RoleTypes.Impostor;

        CleanBodyCd = CustomOption.Create(false, CustomOption.CustomOptionType.Impostor, 
            LanguageConfig.Instance.CleanBodyCooldown, 30f, 1f, 60f, 1f, MainRoleOption);

        CleanBodyButton = CustomButton.Create(
            () =>
            {
                var body = PlayerUtils.GetClosestBody();
                if (!body) return;
                RpcCleanDeadBody(body!);
            },
            () => CleanBodyButton?.ResetCooldown(),
            couldUse: () => true,
            () => true,
            ResourceUtils.LoadSpriteFromResources("COG.Resources.InDLL.Images.Buttons.CleanDeadBody.png", 100f)!,
            row: 2,
            KeyCode.C,
            LanguageConfig.Instance.CleanAction,
            (Cooldown)CleanBodyCd!.GetFloat,
            0
        );

        AddButton(CleanBodyButton);
    }

    public void RpcCleanDeadBody(DeadBody body)
    {
        var writer = RpcUtils.StartRpcImmediately(PlayerControl.LocalPlayer, KnownRpc.CleanDeadBody);
        writer.Write(body.ParentId);
        writer.Finish();
        CleanDeadBody(body);
    }

    public void CleanDeadBody(DeadBody body) => body.gameObject.SetActive(false); // idk why it make PlayerControl.FixedUpdate() throw System.NullReferenceException when i destroy the body

    [EventHandler(EventHandlerType.Postfix)]
    public void OnRPCReceived(PlayerHandleRpcEvent @event)
    {
        var callId = @event.CallId;
        var reader = @event.MessageReader;
        if (callId != (byte)KnownRpc.CleanDeadBody) return;
        var pid = reader.ReadByte();
        var body = Object.FindObjectsOfType<DeadBody>().ToList().FirstOrDefault(b => b.ParentId == pid);
        if (!body) return;
        CleanDeadBody(body!);
    }

    public override IListener GetListener(PlayerControl player) => this;
}
