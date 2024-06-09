using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.Rpc;
using COG.UI.CustomOption;
using COG.Utils;
using COG.Utils.Coding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COG.Role.Impl.SubRole;

[NotTested]
[NotUsed]
public class Lover : CustomRole, IListener
{
    public static Dictionary<PlayerControl, PlayerControl> Couples { get; private set; }
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

    public void RpcSyncLovers()
    {
        var writer = RpcUtils.StartRpcImmediately(PlayerControl.LocalPlayer, KnownRpc.SyncLovers);

        writer.WritePacked(Lover.Couples.Count);
        foreach (var (p1, p2) in Lover.Couples!)
            writer.Write(p1.PlayerId).Write(p2.PlayerId);

        writer.Finish();
    }


    [EventHandler(EventHandlerType.Postfix)]
    public void OnReceiveRpc(PlayerHandleRpcEvent @event)
    {
        var id = (KnownRpc)@event.CallId;
        var reader = @event.MessageReader;
        if (id != KnownRpc.SyncLovers) return;

        Couples.Clear();
        int count = reader.ReadPackedInt32();

        for (int i = 0; i < count; i++)
        {
            var p1 = PlayerUtils.GetPlayerById(reader.ReadByte())!;
            var p2 = PlayerUtils.GetPlayerById(reader.ReadByte())!;
            Couples.Add(p1, p2);
        }
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnPlayerMurderLoverSuicide(PlayerMurderEvent @event)
    {
        var victim = @event.Target;
        if (!victim.IsInLove()) return;

        var lover = victim.GetLover()!;
        lover.LocalDieWithReason(lover, Utils.DeathReason.LoverSuicide); // Everyone will know that another lover is dead, so calling local murdering method is OK
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnEjectionLoverSuicide(PlayerExileBeginEvent @event)
    {
        var exiled = @event.Exiled?.Object;
        if (!(exiled && exiled!.IsInLove())) return;

        var lover = exiled!.GetLover()!;
        lover.LocalDieWithReason(lover, Utils.DeathReason.LoverSuicide, false);
    }


    public override bool OnRoleSelection(List<CustomRole> roles)
    {
        for (int i = 0; i < (RoleNumberOption?.GetQuantity() ?? 1) * 2; i++) 
            roles.Add(this);
        return true;
    }

    public override void AfterSharingRoles()
    {
        var players = Players.Disarrange();
        if (players.Count % 2 != 0)
        {
            Main.Logger.LogError("Couldn't assign lovers, game will end now.");
            return;
        }

        for (int i = 0; i < players.Count / 2; i+=2)
        {
            var p1 = players[i];
            var p2 = players[i + 1];
            Couples!.Add(p1, p2);
        }

        RpcSyncLovers();
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
        Couples.Clear();
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