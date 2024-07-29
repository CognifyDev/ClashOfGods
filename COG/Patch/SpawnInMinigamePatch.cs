using COG.Listener.Impl;
using SpawnInMinigame_WaitForFinish = SpawnInMinigame._WaitForFinish_d__13;

namespace COG.Patch;

[HarmonyPatch(typeof(SpawnInMinigame_WaitForFinish), nameof(SpawnInMinigame_WaitForFinish.MoveNext))]
internal class SpawnInMinigameBlackoutFixPatch
{
    public static void Postfix(bool __result)
    {
        var hud = HudManager.Instance;
        if (VanillaBugFixListener.OccuredBlackoutOnAirship && !__result)
        {
            hud.StartCoroutine(hud.CoFadeFullScreen(new(0, 0, 0, 1), new(0, 0, 0, 0)));
            hud.PlayerCam.Locked = false;
            VanillaBugFixListener.OccuredBlackoutOnAirship = false;
        }
    }
}