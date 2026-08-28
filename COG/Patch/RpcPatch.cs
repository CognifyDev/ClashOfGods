using System;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.Rpc;

namespace COG.Patch;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
internal class RPCHandlerPatch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId,
        [HarmonyArgument(1)] MessageReader reader)
    {
        if (reader == null) return;

        var name = "";
        if (Enum.IsDefined((RpcCalls)callId))
            name = ((RpcCalls)callId).ToString();
        else if (Enum.IsDefined((KnownRpc)callId))
            name = ((KnownRpc)callId).ToString();
        else
            name = callId.ToString();
        Main.Logger.LogDebug($"Rpc {name}({callId}) received, rpc length => {reader.Length}");

        var rpcEvent = new PlayerHandleRpcEvent(__instance, callId, reader);
        try
        {
            ListenerManager.GetManager()
                .ExecuteHandlers(rpcEvent, EventHandlerType.Postfix);
        }
        finally
        {
            rpcEvent.Recycle();
        }
    }

    [HarmonyPrefix]
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId,
        [HarmonyArgument(1)] MessageReader reader)
    {
        var rpcEvent = new PlayerHandleRpcEvent(__instance, callId, reader);
        try
        {
            return ListenerManager.GetManager().ExecuteHandlers(rpcEvent, EventHandlerType.Prefix);
        }
        finally
        {
            rpcEvent.Recycle();
        }
    }
}