using System.Linq;
using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.UI.CustomOption;
using COG.Utils;
using COG.Utils.Coding;

namespace COG.Role.Impl.Neutral;

[NotUsed]
[NotTested]
public class Sidekick : CustomRole, IListener
{
    public Sidekick() : base(LanguageConfig.Instance.SidekickName,
        CustomRoleManager.GetManager().GetTypeRoleInstance<Jackal>().Color, CampType.Neutral, false)
    {
        BaseRoleType = RoleTypes.Crewmate;
        CanVent = true;
        Description = LanguageConfig.Instance.SidekickDescription;

        if (ShowInOptions)
            SidekickCanCreateSidekick = CustomOption.Create(CustomOption.TabType.Neutral,
                LanguageConfig.Instance.SidekickCanCreateSidekick, true,
                CustomRoleManager.GetManager().GetTypeRoleInstance<Jackal>().MainRoleOption);
    }

    public CustomOption SidekickCanCreateSidekick { get; }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnPlayerMurder(PlayerMurderEvent @event)
    {
        if (!PlayerControl.LocalPlayer.IsRole(this)) return;
        var victim = @event.Target;
        if (victim.IsSamePlayer(Jackal.JackalSidekick
                .FirstOrDefault(kvp => kvp.Value.IsSamePlayer(PlayerControl.LocalPlayer)).Key))
            PlayerControl.LocalPlayer.RpcSetCustomRole<Jackal>();
    }

    public override IListener GetListener()
    {
        return this;
    }
}