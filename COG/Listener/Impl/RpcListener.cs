namespace COG.Listener.Impl;

public class RpcListener : IListener
{
    public void OnRPCReceived(byte callId, MessageReader reader)
    {
        
    }

    public void AfterRPCReceived(byte callId, MessageReader reader)
    {
        Main.Logger.LogInfo($"Rpc {callId} received, rpc length => {reader.Length}");
    }
}