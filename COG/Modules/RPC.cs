using System.Linq;
using COG.Listener;
using COG.UI.CustomOption;

namespace COG.Modules;

public enum CustomRPC
{
    ShareOptions = 200,
    ShareRoles = 300
}
[HarmonyPatch(typeof(PlayerControl),nameof(PlayerControl.HandleRpc))]
class RPCHandler
{
    public static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        var rpc = (CustomRPC)callId;
        switch (rpc)
        {
            case CustomRPC.ShareOptions:
                RPCProcedure.HandleShareOptions(reader.ReadByte(), reader);
                break;
        }
            
        foreach (var listener in ListenerManager.GetManager().GetListeners())
        {
            listener.OnRPCReceived(callId, reader);
        }
    }
}

static class RPCProcedure
{
    public static void HandleShareOptions(byte numberOfOptions, MessageReader reader)
    {
        try
        {
            for (int i = 0; i < numberOfOptions; i++)
            {
                var optionId = reader.ReadPackedUInt32();
                var selection = reader.ReadPackedUInt32();
                var option = CustomOption.Options.First(option => option != null && option.ID == (int)optionId);
                option?.UpdateSelection((int)selection);
            }
        }
        catch (System.Exception e)
        {
            Main.Logger.LogError("Error while deserializing options: " + e.Message);
        }
    }
}