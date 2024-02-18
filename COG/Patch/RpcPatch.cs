using System.Linq;
using COG.Listener;
using COG.NewListener;
using COG.NewListener.Event.Impl.Player;

namespace COG.Patch;

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.HandleRpc))]
internal class LobbyBehaviourHandleRpcPatch
{
    private static void Prefix([HarmonyArgument(0)] byte callId, 
        [HarmonyArgument(1)] MessageReader reader)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners())
        {
            Main.Logger.LogInfo($"Rpc {callId} received, rpc length => {reader.Length}");
            listener.OnLobbyRPCReceived(callId, reader);
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
internal class RPCHandlerPatch
{
    [HarmonyPostfix]
    public static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        Main.Logger.LogInfo($"Rpc {callId} received, rpc length => {reader.Length}");
        foreach (var listener in ListenerManager.GetManager().GetListeners().ToList())
            listener.AfterRPCReceived(callId, reader);
    }

    [HarmonyPrefix]
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId,
        [HarmonyArgument(1)] MessageReader reader)
    {
        var rpcType = (RpcCalls)callId;
        var subReader = MessageReader.Get(reader);
        switch (rpcType)
        {
            case RpcCalls.SendChat:
                var text = subReader.ReadString();
                ListenerManager.GetManager().ExecuteHandlers(new PlayerChatEvent(__instance, text!), EventHandlerType.Postfix);
                break;
        }

        return true;
    }
}