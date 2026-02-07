using System.Collections;
using COG.Constant;
using COG.Rpc;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.UI.Hud.CustomButton;
using COG.Utils;
using Reactor.Utilities;
using UnityEngine;

namespace COG.Role.Impl.Impostor;

public class Troublemaker : CustomRole
{
    private readonly CustomButton _disturbButton;
    private readonly CustomOption _disturbCooldown;
    private readonly CustomOption _disturbDuration;
    private readonly RpcHandler _disturbRpcHandler;
    private GameObject? _commsDown;

    private bool _usedThisRound;

    public Troublemaker()
    {
        _disturbDuration = CreateOption(() => GetContextFromLanguage("disturb-duration"), new FloatOptionValueRule(5f,
            5f, 15f,
            10f, NumberSuffixes.Seconds));
        _disturbCooldown = CreateOption(() => GetContextFromLanguage("disturb-cooldown"), new FloatOptionValueRule(10f,
            5f, 60f,
            30f, NumberSuffixes.Seconds));

        _disturbRpcHandler = new RpcHandler(KnownRpc.TroubleMakerDisturb, () =>
        {
            var useButton = HudManager.Instance.UseButton;

            if (!_commsDown)
            {
                _commsDown = Object.Instantiate(HudManager.Instance.AbilityButton.commsDown, useButton.transform);
                _commsDown.name = "CommsDown";
                _commsDown.transform.localPosition =
                    HudManager.Instance.AbilityButton.commsDown.transform.localPosition;
            }

            Coroutines.Start(CoDisturb());

            IEnumerator CoDisturb()
            {
                var timer = _disturbDuration.GetFloat();

                while (timer > 0)
                {
                    timer -= Time.deltaTime;

                    _commsDown!.SetActive(true);
                    useButton.SetTarget(null);
                    PlayerControl.LocalPlayer.closest = null;

                    yield return null;
                }

                _commsDown!.SetActive(false);
            }
        });

        RegisterRpcHandler(_disturbRpcHandler);

        _disturbButton = CustomButton.Builder("troublemaker-disturb", ResourceConstant.DisturbButton,
                ActionNameContext.GetString("disturb"))
            .OnClick(() =>
            {
                _disturbRpcHandler.PerformAndSend();
                _usedThisRound = true;
            })
            .OnMeetingEnds(() => _usedThisRound = false)
            .CouldUse(() => !_usedThisRound)
            .Cooldown(_disturbCooldown.GetFloat)
            .Build();

        AddButton(_disturbButton);
    }
}