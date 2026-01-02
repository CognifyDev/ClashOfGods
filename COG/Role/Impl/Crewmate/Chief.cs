using System.Linq;
using COG.Config.Impl;
using COG.Constant;
using COG.Rpc;
using COG.UI.Hud.CustomButton;
using COG.UI.Vanilla.KillButton;
using COG.Utils;
using InnerNet;
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

public class Chief : CustomRole
{
    private readonly CustomButton _giveKillButton;

    private readonly RpcHandler<PlayerControl> _giveKillRpcHandler;
    private readonly CustomButton _giveShieldButton;

    private PlayerControl? _target;

    public Chief() : base(Color.gray, CampType.Crewmate)
    {
        _giveKillRpcHandler = new RpcHandler<PlayerControl>(KnownRpc.GiveOneKill,
            player =>
            {
                if (!player.AmOwner) return; // local player only
                if (!player.GetRoles()
                        .Any(r => r.CanKill)) // normally speaking, there should be only one role which can kill
                {
                    var role = player.GetMainRole();

                    role.CurrentKillButtonSetting = new KillButtonSetting
                    {
                        ForceShow = () => true,
                        InitialCooldown = 0,
                        UsesLimit = 1,
                        RemainingUses = 1
                    };

                    role.CurrentKillButtonSetting.AddAfterClick(() =>
                        role.ResetCurrentKillButtonSetting()); // restore setting after use
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
            ResourceUtils.LoadSprite(ResourceConstant.GiveKillButton)!,
            2,
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
            action.GetString("give-shield"),
            () => 0,
            1);

        AddButton(_giveKillButton);
        AddButton(_giveShieldButton);
    }
}