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

        _giveKillButton = CustomButton.Builder("chief-give-kill",
                ResourceConstant.GiveKillButton, action.GetString("give-kill"))
            .OnClick(() => _giveKillRpcHandler.Send(_target!)) // just send
            .CouldUse(() => PlayerControl.LocalPlayer.CheckClosestTargetInKillDistance(out _target))
            .Cooldown(() => 0)
            .UsesLimit(1)
            .Build();
        
        /* why sprite is null? */
        /* allocated the sprite of guardian angel button, see CustomButton.cs */
        _giveShieldButton = CustomButton.Builder("chief-give-shield", (Sprite) null!, action.GetString("give-shield"))
            .OnClick(() => PlayerControl.LocalPlayer.CmdCheckProtect(_target))
            .CouldUse(() => PlayerControl.LocalPlayer.CheckClosestTargetInKillDistance(out _target))
            .Cooldown(() => 0)
            .UsesLimit(1)
            .Build();

        AddButton(_giveKillButton);
        AddButton(_giveShieldButton);
    }
}