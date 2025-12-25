using System;
using System.Collections.Generic;
using System.Linq;

namespace COG.Utils.Player;

public record PlayerCouple(PlayerControl First, PlayerControl Second)
{
    public virtual bool Equals(PlayerCouple? other)
    {
        if (other == null) return false;
        return First.IsSamePlayer(other.First) && Second.IsSamePlayer(other.Second);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(First.PlayerId + First.Data.PlayerName, Second + First.Data.PlayerName);
    }

    private static readonly HashSet<PlayerCouple> Couples = [];

    public static PlayerCouple Of(PlayerControl first, PlayerControl second)
    {
        var couple = Couples.FirstOrDefault(target => target.First.IsSamePlayer(first) && target.Second.IsSamePlayer(second), new PlayerCouple(first, second));
        Couples.Add(couple);
        return couple;
    }
}