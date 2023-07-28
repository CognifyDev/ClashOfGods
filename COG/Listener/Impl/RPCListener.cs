using COG.Utils;
using InnerNet;

namespace COG.Listener.Impl;

public class RPCListener : IListener
{
    public void OnRPCReceived(byte callId, MessageReader reader)
    {
        switch (callId)
        {
            case (byte)KnownRpc.ShareRoles:
                if (AmongUsClient.Instance.AmHost) break;
                var rolesShare = reader.ReadNetObject<GameListener.RolesShare>();
                GameUtils.Data.Clear();
                foreach (var keyValuePair in rolesShare.GetRolesInformation())
                {
                    GameUtils.Data.Add(keyValuePair.Key, keyValuePair.Value);
                }
                break;
        }
    }
}

public enum KnownRpc : byte
{
    ShareRoles = 100
}