using COG.Listener;
using COG.Listener.Event.Impl.Player;

namespace COG.Patch;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
internal class RPCHandlerPatch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        Main.Logger.LogInfo($"Rpc {callId} received, rpc length => {reader.Length}");
        ListenerManager.GetManager().ExecuteHandlers(new PlayerHandleRpcEvent(__instance, callId, reader), EventHandlerType.Postfix);
    }

    [HarmonyPrefix]
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId,
        [HarmonyArgument(1)] MessageReader reader)
    {
        var result = ListenerManager.GetManager().ExecuteHandlers(new PlayerHandleRpcEvent(__instance, callId, reader), EventHandlerType.Prefix);
        var rpcType = (RpcCalls)callId;
        var subReader = MessageReader.Get(reader);
        if (RpcCalls.SendChat.Equals(rpcType))
        {
            var text = subReader.ReadString();
            ListenerManager.GetManager().ExecuteHandlers(new PlayerChatEvent(__instance, text!), EventHandlerType.Postfix);
        }

        return result;
    }
}