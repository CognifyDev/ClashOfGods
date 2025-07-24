using COG.Rpc;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using COG.Utils.Coding;
using Reactor.Utilities;
using System.Collections;
using UnityEngine;

namespace COG.Role.Impl.Impostor;

public class Troublemaker : CustomRole
{
    private CustomOption _disturbDuration;
    private CustomOption _disturbCooldown;
    private CustomButton _disturbButton;
    private RpcHandler _disturbRpcHandler;
    private GameObject? _commsDown;

    private bool _usedThisRound = false;

    public Troublemaker() : base()
    {
        _disturbDuration = CreateOption(() => GetContextFromLanguage("disturb-duration"), new FloatOptionValueRule(5f, 5f, 15f,
            10f, NumberSuffixes.Seconds));
        _disturbCooldown = CreateOption(() => GetContextFromLanguage("disturb-cooldown"), new FloatOptionValueRule(10f, 5f, 60f,
            30f, NumberSuffixes.Seconds));

        _disturbRpcHandler = new(KnownRpc.TroubleMakerDisturb, () =>
        {
            var useButton = HudManager.Instance.UseButton;

            if (!_commsDown)
            {
                _commsDown = Object.Instantiate(HudManager.Instance.AbilityButton.commsDown, useButton.transform);
                _commsDown.name = "CommsDown";
                _commsDown.transform.localPosition = HudManager.Instance.AbilityButton.commsDown.transform.localPosition;
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

        _disturbButton = CustomButton.Of("troublemaker-disturb",
            () =>
            {
                _disturbRpcHandler.PerformAndSend();
                _usedThisRound = true;
            },
            () => _usedThisRound = false,
            () => !_usedThisRound,
            () => true,
            null!,
            2,
            KeyCode.B,
            ActionNameContext.GetString("disturb"),
            () => _disturbCooldown.GetFloat(),
            0);

        AddButton(_disturbButton);
    }
}
