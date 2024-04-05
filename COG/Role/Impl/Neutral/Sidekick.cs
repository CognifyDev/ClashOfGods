using AmongUs.GameOptions;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.UI.CustomOption;
using COG.Utils;
using COG.Utils.Coding;
using System.Linq;

namespace COG.Role.Impl.Neutral;

[NotUsed]
[Unfinished]
public class Sidekick : Role, IListener
{
    public CustomOption SidekickCanCreateSidekick { get; }
    public Sidekick() : base("", RoleManager.GetManager().GetTypeRoleInstance<Jackal>().Color, CampType.Neutral, false)
    {
        BaseRoleType = RoleTypes.Crewmate;
        CanVent = true;

        if (ShowInOptions)
        {
            SidekickCanCreateSidekick = CustomOption.Create(false, CustomOption.CustomOptionType.Neutral, "", true, MainRoleOption);
        }
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnPlayerMurder(PlayerMurderEvent @event)
    {
        if (!PlayerControl.LocalPlayer.IsRole(this)) return;
        var victim = @event.Target;
        if (victim.IsSamePlayer(Jackal.JackalSidekick.FirstOrDefault(kvp => kvp.Value.IsSamePlayer(PlayerControl.LocalPlayer)).Key))
            PlayerControl.LocalPlayer.RpcSetCustomRole<Jackal>();
    }
}