using COG.Config.Impl;
using COG.Utils;
using UnityEngine;

namespace COG.Patch;

[HarmonyPatch]
// ChatBubble methods order: (Init) => SetRight/SetLeft => SetCosmetics => SetChatBubbleName (called SetName) => SetText => AlignChildren => (Done)
internal static class ChatPlayerInfoPatch
{
    [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetText))]
    [HarmonyPostfix]
    private static void SetTextPatch(ChatBubble __instance)
    {
        if (__instance.NameText.text == LanguageConfig.Instance.SystemMessage)
            __instance.SetLeft();
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SetChatBubbleName))]
    [HarmonyPrefix]
    private static void SetChatBubbleNamePatch(NetworkedPlayerInfo playerInfo, ref Color nameColor)
    {
        if (GameStates.InLobby) return; // Role not assigned, causes exceptions
        if (playerInfo.AmOwner || !PlayerControl.LocalPlayer.IsAlive())
            nameColor = playerInfo.GetMainRole().Color;
    }
}

[HarmonyPatch]
internal static class ChatBubblePoolPatch
{
    public static int MaxBubbleCount { get; set; }

    [HarmonyPatch(typeof(ObjectPoolBehavior), nameof(ObjectPoolBehavior.InitPool))]
    [HarmonyPostfix]
    private static void PoolSizePatch(ObjectPoolBehavior __instance, [HarmonyArgument(0)] PoolableBehavior prefab)
    {
        ChatController? chat = null;

        if ((chat = Object.FindObjectOfType<ChatController>()) && chat.chatBubblePool == __instance)
        {
            var pool = chat.chatBubblePool;
            var total = pool.inactiveChildren.Count + pool.activeChildren.Count;
            var delta = MaxBubbleCount - total;

            __instance.poolSize = MaxBubbleCount;

            for (var i = 0; i < delta; i++)
                __instance.CreateOneInactive(prefab);
        }
    }
}