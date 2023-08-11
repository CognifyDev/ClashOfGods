using System.Linq;
using COG.Config.Impl;
using COG.Listener;
using COG.UI.CustomOption;
using COG.UI.CustomWinner;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.Neutral;

public class Jester : Role, IListener, ICustomWinner
{
    private PlayerControl? _player;

    public Jester() : base(LanguageConfig.Instance.JesterName, Color.magenta, CampType.Neutral, true)
    {
        Description = LanguageConfig.Instance.JesterDescription;
        var parentOption = RoleOptions[0];
        RoleOptions.Add(CustomOption.Create(
            parentOption.ID + 1, ToCustomOption(this), LanguageConfig.Instance.AllowStartMeeting, true, parentOption)
        );
        RoleOptions.Add(CustomOption.Create(
            parentOption.ID + 2, ToCustomOption(this), LanguageConfig.Instance.AllowReportDeadBody, true, parentOption)
        );

        CustomWinnerManager.RegisterCustomWinnerInstance(this);
    }

    public bool CanWin()
    {
        var jester = DeadPlayerManager.DeadPlayers.FirstOrDefault(dp =>
            dp.Role == RoleManager.GetManager().GetTypeRoleInstance<Jester>() &&
            dp.DeathReason == Utils.DeathReason.Exiled);
        if (jester == null) return true;
        GameManager.Instance.RpcEndGame(GameOverReason.HumansByVote, false);
        CustomWinnerManager.RegisterCustomWinner(jester.Player);
        return false;
    }

    public ulong GetWeight()
    {
        return ICustomWinner.GetOrder(4);
    }

    public bool OnPlayerReportDeadBody(PlayerControl playerControl, GameData.PlayerInfo? target)
    {
        var allowStartMeetingOption = RoleOptions[2];
        var allowReportDeadBodyOption = RoleOptions[3];
        var result1 = allowStartMeetingOption.GetBool();
        var result2 = allowReportDeadBodyOption.GetBool();
        if (!result1 && playerControl.IsSamePlayer(_player!) && target == null) return false;

        return result2 || playerControl.IsSamePlayer(_player!) || target == null;
    }

    private bool CheckNull()
    {
        return _player == null;
    }

    public override IListener GetListener(PlayerControl player)
    {
        _player = player;
        return this;
    }
}