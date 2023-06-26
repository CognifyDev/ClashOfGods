using COG.Listener;
using HarmonyLib;

namespace COG.Patch;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
class HostChatPatch
{
    public static bool Prefix(ChatController __instance)
    {
        if (__instance.TextArea.text == "") return false;

        bool returnAble = false;
        foreach (var listener in ListenerManager.GetManager().GetListeners())
        {
            if (!listener.OnHostChat(__instance) && !returnAble)
            {
                returnAble = true;
            }
        }

        if (returnAble) return false;

        return true;
    }
}


[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
class PlayerMurderPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost) return false;
        
        bool returnAble = false;
        foreach (var listener in ListenerManager.GetManager().GetListeners())
        {
            if (!listener.OnPlayerMurder(__instance, target) && !returnAble)
            {
                returnAble = true;
            }
        }

        if (returnAble) return false;

        return true;
    }
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
internal class ChatUpdatePatch
{
    public static void Postfix(ChatController __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        foreach (var listener in ListenerManager.GetManager().GetListeners())
        {
            listener.OnChatUpdate(__instance);
        }
    }
}