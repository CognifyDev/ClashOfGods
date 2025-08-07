using System.Linq;
using COG.Game.Events;
using COG.Role.Impl.Impostor;
using COG.Utils;

namespace COG.Listener.Event.Impl.Game.Handlers;

public class PlayerKillHandler : TypeEventHandlerBase
{
    public PlayerKillHandler() : base(GameEventType.Kill)
    {
    }

    public override IGameEvent? Handle(CustomPlayerData player, params object[] extraArguments)
    {
        if (extraArguments.Length == 2)
        {
            if (extraArguments[0] is not CustomPlayerData victim || extraArguments[1] is not string extraInfo)
            {
                Main.Logger.LogWarning(
                    $"Invalid {nameof(extraArguments)} cause {nameof(victim)} or {nameof(extraInfo)} being null. ({extraArguments[0]}, {extraArguments[1]})");
                return null!;
            }

            if (extraInfo.StartsWith(Stabber.ModifyKillAnimMessage))
            {
                if (byte.TryParse(extraInfo.Replace(Stabber.ModifyKillAnimMessage, ""), out var id))
                {
                    player = GameUtils.PlayerData.FirstOrDefault(p =>
                        p.PlayerId == id)!; // Override killer for killAnimation
                    if (player == null)
                    {
                        Main.Logger.LogWarning("Invalid player id");
                        return null!;
                    }
                }
                else
                {
                    Main.Logger.LogWarning($"Invalid extra argument: {extraInfo}");
                    return null!;
                }
            }

            return new PlayerKillGameEvent(player, victim);
        }

        return null!;
    }
}