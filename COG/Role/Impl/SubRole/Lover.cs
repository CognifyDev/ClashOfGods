using COG.Listener;
using COG.UI.CustomOption;
using COG.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COG.Role.Impl.SubRole;

public class Lover : CustomRole, IListener
{
    public static Dictionary<PlayerControl, PlayerControl>? Couples { get; private set; }
    private CustomOption LoversDieTogetherOption { get; }
    private CustomOption EnablePrivateChatOption { get; }
    public Lover() : base("Lover", UnityEngine.Color.magenta, CampType.Unknown)
    {
        IsSubRole = true;
        Couples = new();
        if (ShowInOptions)
        {
            var tab = ToCustomOption(this);
            RoleNumberOption!.Name = "LoverCountOption";
            LoversDieTogetherOption = CustomOption.Create(tab, "LoversDieTogether", true, MainRoleOption);
            EnablePrivateChatOption = CustomOption.Create(tab, "EnablePrivateChat", true, MainRoleOption);
        }
    }

    public override bool OnRoleSelection(List<CustomRole> roles)
    {
        for (int i = 0; i < (RoleNumberOption?.GetQuantity() ?? 1) * 2; i++) 
            roles.Add(this);
        
        return true;
    }

    public override string HandleAdditionalPlayerName()
    {
        var lover = PlayerControl.LocalPlayer.GetLover();
        if (!lover) return "";
        return $"\n♥{lover!.Data.PlayerName.Color(Palette.White)}♥";
    }

    public override string HandleEjectText(PlayerControl player)
    {
        string msg = "";

        if (LoversDieTogetherOption?.GetBool() ?? true)
        {
            var sb = new StringBuilder(player.GetMainRole().GetColorName()).Append(' ');
            foreach (var sub in player.GetSubRoles()) sb.Append(sub.GetColorName()).Append(' ');

            msg = "LoverDiedTogetherMessage %exiled% %roles% %loverSuicide%".CustomFormat(player, sb.ToString(), player.GetLover()!.Data.PlayerName);
        }
        else
            msg = base.HandleEjectText(player);
        
        return msg;
    }

    public override void ClearRoleGameData()
    {
        Couples?.Clear();
    }

    public override IListener GetListener() => this;
}

public static partial class RoleUtils
{
    public static bool IsInLove(this PlayerControl player, PlayerControl? other = null)
    {
        if (!other)
            return Lover.Couples!.Any(c => c.Key.IsSamePlayer(player) || c.Value.IsSamePlayer(player));
        else
            return Lover.Couples!.Any(c => c.Equals(new KeyValuePair<PlayerControl, PlayerControl>(player, other!)) || c.Equals(new KeyValuePair<PlayerControl, PlayerControl>(other!, player)));
    }

    public static PlayerControl? GetLover(this PlayerControl player)
    {
        foreach (var (p1, p2) in Lover.Couples!)
            if (p1.IsSamePlayer(player))
                return p2;
            else if (p2.IsSamePlayer(player))
                return p1;

        return null;
    }
} 