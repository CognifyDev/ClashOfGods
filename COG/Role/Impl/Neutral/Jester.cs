using System.Linq;
using COG.Config.Impl;
using COG.Game.CustomWinner;
using COG.Listener;
using COG.UI.CustomOption;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.Neutral;

public class Jester : Role, IListener, IWinnable
{
    public readonly CustomOption AllowStartMeeting, AllowReportDeadBody;

    public static Jester JesterInstance { get; private set; } = null!;

    public Jester() : base(LanguageConfig.Instance.JesterName, Color.magenta, CampType.Neutral, true)
    {
        Description = LanguageConfig.Instance.JesterDescription;
        AllowStartMeeting = CustomOption.Create(
            false, ToCustomOption(this), LanguageConfig.Instance.AllowStartMeeting, true,
            MainRoleOption);
        AllowReportDeadBody = CustomOption.Create(
            false, ToCustomOption(this), LanguageConfig.Instance.AllowReportDeadBody, true,
            MainRoleOption);

        CustomWinnerManager.RegisterCustomWinnerInstance(this);
        JesterInstance = this;
    }

    public bool CanWin()
    {
        var jester = DeadPlayerManager.DeadPlayers.FirstOrDefault(dp =>
            dp.Role == RoleManager.GetManager().GetTypeRoleInstance<Jester>() &&
            dp.DeathReason == Utils.DeathReason.Exiled);
        if (jester == null) return false;
        GameManager.Instance.RpcEndGame(GameOverReason.HumansByVote, false);
        CustomWinnerManager.RegisterCustomWinner(jester.Player);
        return true;
    }

    public ulong GetWeight()
    {
        return IWinnable.GetOrder(5);
    }

    public override IListener GetListener(PlayerControl player) => new JesterListener(player);
}

public class JesterListener : IListener
{
    private readonly PlayerControl _player;
    public JesterListener(PlayerControl playerControl)
    {
        _player = playerControl;
    }
    
    public bool OnPlayerReportDeadBody(PlayerControl playerControl, GameData.PlayerInfo? target)
    {
        var result1 = Jester.JesterInstance.AllowStartMeeting.GetBool();
        var result2 = Jester.JesterInstance.AllowReportDeadBody.GetBool();
        if (!result1 && playerControl.IsSamePlayer(_player) && target == null) return false;

        return result2 || playerControl.IsSamePlayer(_player) || target == null;
    }
}