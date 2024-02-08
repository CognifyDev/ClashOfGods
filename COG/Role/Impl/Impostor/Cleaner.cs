using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Listener;
using COG.Rpc;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.Utils;
using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace COG.Role.Impl.Impostor
{
    public class Cleaner : Role, IListener
    {
        public CustomOption CleanBodyCD { get; init; } = new();
        public CustomButton CleanBodyButton { get; init; } = new();
        public Cleaner() : base(LanguageConfig.Instance.CleanerName, Palette.ImpostorRed, CampType.Impostor, true)
        {
            Description = LanguageConfig.Instance.CleanerDescription;
            BaseRoleType = RoleTypes.Impostor;

            CleanBodyButton = CustomButton.Create(
                () =>
                {
                    var body = PlayerUtils.GetClosestBody();
                    if (!body) return;
                    RpcCleanDeadBody(body!);
                },
                CleanBodyButton.ResetCooldown,
                null,
                null,
                ResourceUtils.LoadSpriteFromResources("COG.Resources.InDLL.Images.Buttons.CleanDeadBody.png", 100f)!,
                2,
                KeyCode.C,
                LanguageConfig.Instance.CleanAction,
                CleanBodyCD.GetFloat,
                0
            );

            AddButton(CleanBodyButton);

            if (ShowInOptions)
            {
                var headerID = MainRoleOption!.ID;
                CleanBodyCD = CustomOption.Create(headerID + 1, CustomOption.CustomOptionType.Impostor, LanguageConfig.Instance.CleanBodyCooldown, 30f, 30f, 60f, 5f, MainRoleOption);
            }
        }

        public void RpcCleanDeadBody(DeadBody body)
        {
            var writer = RpcUtils.StartRpcImmediately(PlayerControl.LocalPlayer, KnownRpc.CleanDeadBody);
            writer.Write(body.ParentId);
            writer.Finish();
            CleanDeadBody(body);
        }

        public void CleanDeadBody(DeadBody body) => body.gameObject.SetActive(false); // idk why it make PlayerControl.FixedUpdate() throw System.NullReferenceException when i destroy the body

        public void OnRPCReceived(byte callId, MessageReader reader)
        {
            if (callId == (byte)KnownRpc.CleanDeadBody)
            {
                byte pid = reader.ReadByte();
                var body = Object.FindObjectsOfType<DeadBody>().ToList().FirstOrDefault(b => b.ParentId == pid);
                if (!body) return;
                CleanDeadBody(body!);
            }
        }

        public override IListener GetListener(PlayerControl player)
        {
            return this;
        }
    }
}
