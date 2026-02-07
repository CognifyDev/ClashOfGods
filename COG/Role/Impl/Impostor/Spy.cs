using System.Collections;
using COG.Constant;
using COG.Rpc;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.UI.Hud.Arrow;
using COG.UI.Hud.CustomButton;
using COG.UI.Hud.CustomMessage;
using COG.Utils;
using COG.Utils.Coding;
using Reactor.Utilities;
using UnityEngine;

namespace COG.Role.Impl.Impostor;

[NotTested("队友的HudMessage")]
public class Spy : CustomRole
{
    private readonly CustomButton _observeButton;
    private readonly CustomOption _observeCooldown;
    private readonly RpcHandler<bool> _revealClosestTargetHandler;

    public Spy()
    {
        _observeCooldown = CreateOption(() => GetContextFromLanguage("observe-cooldown"),
            new FloatOptionValueRule(10, 5, 60, 20, NumberSuffixes.Seconds));

        _revealClosestTargetHandler = new RpcHandler<bool>(KnownRpc.SpyRevealClosestTarget,
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
                        CustomHudMessage.Instance.AddMessage(
                            GetContextFromLanguage(isByKill
                                ? "get-target-arrow-message-kill"
                                : "get-target-arrow-message-observe"), arrowTime);

                    while (timer > 0)
                    {
                        timer -= Time.deltaTime;
                        arrow.target = closest.transform.position;
                        yield return null;
                    }

                    arrow.gameObject.TryDestroy();
                }
            },
            (writer, isByKill) => writer.Write(isByKill),
            reader => reader.ReadBoolean());

        RegisterRpcHandler(_revealClosestTargetHandler);

        _observeButton = CustomButton.Builder("spy-observe",
                ResourceConstant.ObserveButton,
                ActionNameContext.GetString("observe"))
            .OnClick(() => _revealClosestTargetHandler.PerformAndSend(false))
            .Cooldown(_observeCooldown.GetFloat)
            .Build();

        AddButton(_observeButton);

        CurrentKillButtonSetting.AddAfterClick(() => _revealClosestTargetHandler.PerformAndSend(true));
    }
}