using COG.Rpc;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.Impostor;

public class Spy : CustomRole
{
    private CustomOption _observeCooldown;
    private RpcHandler _revealClosestTargetHandler;
    private CustomButton _observeButton;

    public Spy() : base()
    {
        _observeCooldown = CreateOption(()=>GetContextFromLanguage("observe-cooldown"),
            new FloatOptionValueRule(10, 5, 60, 20, NumberSuffixes.Seconds));

        _revealClosestTargetHandler = new(KnownRpc.SpyRevealClosestTarget,
            () =>
            {
                if (PlayerControl.LocalPlayer.GetMainRole().CampType != CampType.Impostor) return;
                var closest = PlayerControl.LocalPlayer.GetClosestPlayer(includeImpostor: false);
                if (!closest) return;
                var distance = Vector2.Distance(PlayerControl.LocalPlayer.GetTruePosition(), closest!.GetTruePosition());
                // TODO: TEXT SHOWING DISTANCE
            });

        RegisterRpcHandler(_revealClosestTargetHandler);

        _observeButton = CustomButton.Of("spy-observe",
            () => _revealClosestTargetHandler.PerformAndSend(),
            () => { },
            () => true,
            () => true,
            null!,
            2,
            ActionNameContext.GetString("observe"),
            () => _observeCooldown.GetFloat(),
            0);

        AddButton(_observeButton);
    }
}
