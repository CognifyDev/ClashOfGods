namespace COG.UI.Load
{
    [HarmonyPatch(typeof(LoadingBarManager))]
    public class LoadingBarManagerPatch
    {
        [HarmonyPatch(nameof(LoadingBarManager.ToggleLoadingBar))]
        public static void Prefix(LoadingBarManager __instance, ref bool on)
        {
            __instance.loadingBar.crewmate.gameObject.SetActive(false);
            try
            {
                if (AmongUsClient.Instance.GameState != AmongUsClient.GameStates.NotJoined) return;
                on = false;
            }
            catch
            {
                on = false;
            }
        }
    }
}
