using COG.Role.Impl.Crewmate;
using COG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Game.Events.Impl;

public class PlayerKillEvent : GameEventBase
{
    public CustomPlayerData Victim { get; }

    public PlayerKillEvent(CustomPlayerData killer, CustomPlayerData victim) : base(EventType.Kill, killer)
    {
        Victim = victim;
    }
}
