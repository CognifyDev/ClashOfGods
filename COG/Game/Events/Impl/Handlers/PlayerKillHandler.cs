using COG.Utils;

namespace COG.Game.Events.Impl.Handlers;

public class PlayerKillHandler : TypeEventHandlerBase
{
    public PlayerKillHandler() : base(EventType.Kill)
    {
    }

    public override IGameEvent Handle(CustomPlayerData player, params object[] extraArguments)
    {
        if (extraArguments.Length < 2 || 
           extraArguments[0] is not CustomPlayerData victim)
        {
            Main.Logger.LogWarning("Invalid event arguments");
            return null!;
        }

        return new PlayerKillEvent(player, victim);
    }
}
