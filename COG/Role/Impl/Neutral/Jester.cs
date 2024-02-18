using System.Linq;
using COG.Config.Impl;
using COG.Game.CustomWinner;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.NewListener;
using COG.UI.CustomOption;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.Neutral;

public class Jester : Role, IListener, IWinnable
{
    private readonly CustomOption _allowStartMeeting;
    private readonly CustomOption _allowReportDeadBody;

    public Jester() : base(LanguageConfig.Instance.JesterName, Color.magenta, CampType.Neutral, true)
    {
        Description = LanguageConfig.Instance.JesterDescription;
        _allowStartMeeting = CustomOption.Create(
            false, ToCustomOption(this), LanguageConfig.Instance.AllowStartMeeting, true,
            MainRoleOption);
        _allowReportDeadBody = CustomOption.Create(
            false, ToCustomOption(this), LanguageConfig.Instance.AllowReportDeadBody, true,
            MainRoleOption);

        CustomWinnerManager.RegisterCustomWinnerInstance(this);
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
    
    [EventHandler(EventHandlerType.Prefix)]
    public bool OnPlayerReportDeadBody(PlayerReportDeadBodyEvent @event)
    {
        var reportedPlayer = @event.Target;
        var player = @event.Player;
        if (!Id.Equals(player.GetRoleInstance()?.Id)) return true;
        var result1 = _allowStartMeeting.GetBool();
        var result2 = _allowReportDeadBody.GetBool();
        if (!result1 && reportedPlayer == null) return false;
        return result2 || reportedPlayer == null;
    }

    public override IListener GetListener(PlayerControl player) => this;
}