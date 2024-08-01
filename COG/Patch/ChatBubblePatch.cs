using COG.Config.Impl;

namespace COG.Patch;

[HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetRight))]
internal static class ChatBubbleSetRightPatch
{
    private static bool Prefix(ChatBubble __instance)
    {
        if (__instance.NameText.text == LanguageConfig.Instance.SystemMessage)
        {
            __instance.SetLeft();
            return false;
        }

        return true;
    }
}