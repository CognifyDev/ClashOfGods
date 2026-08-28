using System.Collections;
using System.Linq;
using COG.Config.Impl;
using COG.Listener;
using COG.Listener.Attribute;
using COG.Listener.Event.Impl.Player;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using Reactor.Utilities;
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

public class SoulHunter : COG.Role.Camp.CrewmateRole
{
    private const string HasRevivedTag = "hasRevived_SoulHunter";

    private Vector2? _position;

    public SoulHunter() : base(Color.green)
    {
        ReviveAfter = CreateOption(() => LanguageConfig.Instance.GetString("role.crewmate.soul-hunter.revive-after"),
            new FloatOptionValueRule(1F, 1F, 60F, 5F, NumberSuffixes.Seconds));
        SoulHunterKillCd = CreateOption(() => LanguageConfig.Instance.GetString("role.global.kill-cooldown"),
            new FloatOptionValueRule(1F, 1F, 60F, 20F, NumberSuffixes.Seconds));

        CanKill = true;

        DefaultKillButtonSetting.UsesLimit = 1;
        DefaultKillButtonSetting.CustomCooldown = SoulHunterKillCd.GetFloat;

        On<PlayerMurderEvent>(OnPlayerMurder);
        On<PlayerReportDeadBodyEvent>(OnReportBody);
    }

    private CustomOption ReviveAfter { get; }
    private CustomOption SoulHunterKillCd { get; }

    public override void ClearRoleGameData()
    {
        _position = null;
    }

    [LocalOnly]
    private void OnPlayerMurder(PlayerMurderEvent @event)
    {
        var target = @event.Target;

        if (target.HasMarkAs(HasRevivedTag)) return;

        _position = target.GetTruePosition();
        Coroutines.Start(CoRevive());
        return;

        IEnumerator CoRevive()
        {
            yield return new WaitForSeconds(ReviveAfter.GetFloat());

            var deadBody = target.GetDeadBody();

            if (deadBody != null)
                deadBody.RpcHideDeadBody();
            else
                yield break;

            if (GameStates.IsMeeting)
                yield break;

            if (_position.HasValue)
                target.NetTransform.RpcSnapTo(_position.Value);
            else
                Main.Logger.LogError($"{nameof(_position)} is null while reviving!");

            target.RpcSetCustomRole(this);
            target.RpcRevive();
            target.RpcMark(HasRevivedTag);
            DefaultKillButtonSetting.RemainingUses++;
        }
    }

    private void OnReportBody(PlayerReportDeadBodyEvent @event)
    {
        var targets = Players.Where(player => player.HasMarkAs(HasRevivedTag));
        targets.ForEach(target => target.RpcSuicide());
    }

    public override string GetNameInConfig()
    {
        return "soul-hunter";
    }
}