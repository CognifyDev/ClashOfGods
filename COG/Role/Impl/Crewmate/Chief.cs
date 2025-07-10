using COG.Config.Impl;
using COG.Rpc;
using COG.UI.CustomButton;
using COG.UI.Vanilla.KillButton;
using COG.Utils;
using COG.Utils.Coding;
using InnerNet;
using System.Linq;
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

public class Chief : CustomRole
{
    private CustomButton _giveKillButton;
    private CustomButton _giveShieldButton;

    private RpcHandler<PlayerControl> _giveKillRpcHandler;

    private PlayerControl? _target;

    public Chief() : base(ColorUtils.FromColor32(75, 86, 107), CampType.Crewmate)
    {
        _giveKillRpcHandler = new RpcHandler<PlayerControl>(KnownRpc.GiveOneKill,
            player =>
            {
                if (!player.AmOwner) return; // local player only
                if (!player.GetRoles().Any(r => r.CanKill)) // normally speaking, there should be only one role which can kill
                {
                    var role = player.GetRoles().FirstOrDefault(r => r.CanKill);

                    role!.CurrentKillButtonSetting = new()
                    {
                        ForceShow = () => true,
                        InitialCooldown = 0,
                        UsesLimit = 1
                    };

                    role!.CurrentKillButtonSetting = role!.DefaultKillButtonSetting; // restore setting after use
                }
            },
            (writer, player) => writer.WriteNetObject(player),
            reader => reader.ReadNetObject<PlayerControl>());

        RegisterRpcHandler(_giveKillRpcHandler);

        var action = new LanguageConfig.TextHandler("action");

        _giveKillButton = CustomButton.Of("chief-give-kill",
            () => _giveKillRpcHandler.Send(_target!), // just send
            () => { },
            () => PlayerControl.LocalPlayer.CheckClosestTargetInKillDistance(out _target),
            () => true,
            null!,
            2,
            KeyCode.K,
            action.GetString("give-kill"),
            () => 0,
            1);

        _giveShieldButton = CustomButton.Of("chief-give-shield",
            () => PlayerControl.LocalPlayer.CmdCheckProtect(_target),
            () => { },
            () => PlayerControl.LocalPlayer.CheckClosestTargetInKillDistance(out _target),
            () => true,
            null!, // allocated the sprite of guardian angel button, see CustomButton.cs
            2,
            KeyCode.H,
            action.GetString("give-shield"),
            () => 0,
            1);

        AddButton(_giveKillButton);
        AddButton(_giveShieldButton);
    }
}