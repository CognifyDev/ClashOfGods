using System.Collections.Generic;
using System.Linq;
using COG.Rpc;
using UnityEngine;

namespace COG.Utils;

public static class DeadBodyUtils
{
    private static readonly int Outline = Shader.PropertyToID("_Outline");
    private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");

    public static List<DeadBody> GetDeadBodies()
    {
        return Object.FindObjectsOfType<DeadBody>().ToList();
    }

    public static void RpcHideDeadBody(this DeadBody body)
    {
        var writer = PlayerControl.LocalPlayer.StartRpcImmediately(KnownRpc.HideDeadBody);
        writer.Write(body.ParentId);
        writer.Finish();

        body.gameObject.SetActive(false);
    }

    public static PlayerControl? GetPlayer(this DeadBody body)
    {
        return PlayerUtils.GetPlayerById(body.ParentId);
    }

    public static void SetOutline(this DeadBody body, Color color)
    {
        body.bodyRenderers.Do(r =>
        {
            r.material.SetInt(Outline, 1);
            r.material.SetColor(OutlineColor, color);
        });
    }

    public static void ClearOutline(this DeadBody body)
    {
        body.bodyRenderers.Do(r => r.material.SetInt(Outline, 0));
    }
}