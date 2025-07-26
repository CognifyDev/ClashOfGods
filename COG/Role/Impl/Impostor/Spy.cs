using COG.Rpc;
using COG.UI.CustomButton;
using COG.UI.CustomGameObject.Arrow;
using COG.UI.CustomGameObject.HudMessage;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using COG.Utils.Coding;
using Reactor.Utilities;
using System.Collections;
using UnityEngine;

namespace COG.Role.Impl.Impostor;

[NotTested]
public class Spy : CustomRole
{
    private CustomOption _observeCooldown;
    private RpcHandler<bool> _revealClosestTargetHandler;
    private CustomButton _observeButton;

    public Spy() : base()
    {
        _observeCooldown = CreateOption(() => GetContextFromLanguage("observe-cooldown"),
            new FloatOptionValueRule(10, 5, 60, 20, NumberSuffixes.Seconds));

        _revealClosestTargetHandler = new(KnownRpc.SpyRevealClosestTarget,
            isByKill =>
            {
                if (PlayerControl.LocalPlayer.GetMainRole().CampType != CampType.Impostor) return;

                var closest = PlayerControl.LocalPlayer.GetClosestPlayer(includeImpostor: false);
                if (!closest) return;

                var arrow = Arrow.Create(closest!.transform.position, Color);
                Coroutines.Start(UpdateTargetArrow());

                IEnumerator UpdateTargetArrow()
                {
                    const float arrowTime = 3f;
                    var timer = arrowTime;

                    if (!IsLocalPlayerRole())
                    {
                        HudMessage.Instance.AddMessage(GetContextFromLanguage(isByKill ? "get-target-arrow-message-kill" : "get-target-arrow-message-observe"), arrowTime);
                    }

                    while (timer > 0)
                    {
                        timer -= Time.deltaTime;
                        arrow.target = closest.transform.position;
                        yield return null;
                    }

                    arrow.Destroy();
                }
            },
            (writer, isByKill) => writer.Write(isByKill),
            reader => reader.ReadBoolean());

        RegisterRpcHandler(_revealClosestTargetHandler);

        _observeButton = CustomButton.Of("spy-observe",
            () => _revealClosestTargetHandler.PerformAndSend(false),
            () => { },
            () => true,
            () => true,
            null!,
            2,
            ActionNameContext.GetString("observe"),
            () => _observeCooldown.GetFloat(),
            0);

        AddButton(_observeButton);

        CurrentKillButtonSetting.AddAfterClick(() => _revealClosestTargetHandler.PerformAndSend(true));
    }
}
