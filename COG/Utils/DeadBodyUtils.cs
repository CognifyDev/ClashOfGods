using System.Collections.Generic;
using System.Linq;
using COG.Rpc;

namespace COG.Utils;

public static class DeadBodyUtils
{
    public static List<DeadBody> GetDeadBodies() => 
        Object.FindObjectsOfType<DeadBody>().ToList();
    
    public static void RpcHideDeadBody(this DeadBody body)
    {
        var writer = RpcUtils.StartRpcImmediately(PlayerControl.LocalPlayer, KnownRpc.HideDeadBody);
        writer.Write(body.ParentId);
        writer.Finish();
        
        body.gameObject.SetActive(false);
    }

    public static PlayerControl? GetPlayer(this DeadBody body)
    {
        return PlayerUtils.GetPlayerById(body.ParentId);
    }
}