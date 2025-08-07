using COG.Game.Events;
using COG.Rpc;
using COG.UI.Hud.CustomButton;
using COG.Utils;

namespace COG.Listener.Event.Impl.Game;

public class UseAbilityGameEvent : NetworkedGameEventBase
{
    public UseAbilityGameEvent(CustomPlayerData player, CustomButton button) : base(GameEventType.UseAbility, player)
    {
        AbilityButtonIdentifier = button.Identifier;

        DoSend();
    }

    public UseAbilityGameEvent(SerializablePlayerData player) : base(GameEventType.UseAbility, player.ToPlayerData())
    {
        // This constructor is used for deserialization, so we don't call DoSend here.
    }

    public string AbilityButtonIdentifier { get; private set; } = null!;

    protected override void Serialize(RpcWriter writer)
    {
        writer.Write(AbilityButtonIdentifier);
    }

    public override void Deserialize(MessageReader reader)
    {
        AbilityButtonIdentifier = reader.ReadString();
    }
}