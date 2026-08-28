using COG.Role.Camp;
using COG.Role.Options;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.Impostor;

public class Reaper : COG.Role.Camp.ImpostorRole
{
    private float _cooldown;
    private FloatOption TimeToReduce { get; }

    public Reaper()
    {
        TimeToReduce = Float("time-to-reduce", 0.5f, 5f, 1.5f, 0.5f);

        _cooldown = GameUtils.GetGameOptions()?.KillCooldown ?? 30f;
        DefaultKillButtonSetting.CustomCooldown = () => _cooldown;

        On<Listener.Event.Impl.Player.PlayerMurderEvent>(OnPlayerMurder);
    }

    private void OnPlayerMurder(Listener.Event.Impl.Player.PlayerMurderEvent @event)
    {
        _cooldown = Mathf.Clamp(_cooldown - TimeToReduce.Value, 1f, float.MaxValue);
    }

    public override void ClearRoleGameData()
    {
        _cooldown = GameUtils.GetGameOptions()?.KillCooldown ?? 30f;
    }
}
