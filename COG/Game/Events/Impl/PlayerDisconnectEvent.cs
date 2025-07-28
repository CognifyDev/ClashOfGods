using COG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Game.Events.Impl;

public class PlayerDisconnectEvent : GameEventBase
{
    public PlayerDisconnectEvent(CustomPlayerData player) : base(EventType.Disconnect, player)
    {
    }
}
