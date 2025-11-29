using COG.Game.Events;
using COG.Rpc;
using COG.UI.Hud.CustomButton;
using COG.Utils;
using System.Linq;

namespace COG.Listener.Event.Impl.Game.Record;

public class UseAbilityGameEvent : NetworkedGameEventBase<UseAbilityGameEvent, UseAbilityEventSender>
{
    public UseAbilityGameEvent(CustomPlayerData player, CustomButton button) : base(GameEventType.UseAbility, player)
    {
        UsedButton = button;
    }

    public CustomButton UsedButton { get; private set; } = null!;
}

public class UseAbilityEventSender : NetworkedGameEventSender<UseAbilityEventSender, UseAbilityGameEvent>
{
    public override void Serialize(RpcWriter writer, UseAbilityGameEvent correspondingEvent)
    {
        writer.WriteBytesAndSize(SerializablePlayerData.Of(correspondingEvent.Player!).SerializeToData());
        writer.Write(correspondingEvent.UsedButton.Identifier);
    }

    public override UseAbilityGameEvent Deserialize(MessageReader reader)
    {
        var rawPlayerData = reader.ReadBytesAndSize();
        var playerData = rawPlayerData.DeserializeToData<SerializablePlayerData>().ToPlayerData();

        var buttonString = reader.ReadString();
        var button = CustomButtonManager.GetManager().GetButtons().First(b => b.Identifier == buttonString);

        return new(playerData, button);
    }
}