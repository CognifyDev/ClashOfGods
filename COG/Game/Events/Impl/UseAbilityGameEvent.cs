using COG.Rpc;
using COG.UI.Hud.CustomButton;
using COG.Utils;

namespace COG.Game.Events.Impl;

public class UseAbilityGameEvent : NetworkedGameEventBase
{
    public string AbilityButtonIdentifier { get; private set; } = null!;

    public UseAbilityGameEvent(CustomPlayerData player, CustomButton button) : base(GameEventType.UseAblity, player)
    {
        AbilityButtonIdentifier = button.Identifier;

        DoSend();
    }

    public UseAbilityGameEvent(SerializablePlayerData player) : base(GameEventType.UseAblity, player.ToPlayerData())
    {
        // This constructor is used for deserialization, so we don't call DoSend here.
    }

    public override void Serialize(RpcWriter writer)
    {
        writer.Write(AbilityButtonIdentifier);
    }

    public override void Deserialize(MessageReader reader)
    {
        AbilityButtonIdentifier = reader.ReadString();
    }
}
