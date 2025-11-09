using COG.UI.ClientOption.Impl;

namespace COG.Patch
{
    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
    public static class CheckEndCriteriaPatch
    {
        public static ToggleClientOption NoEndGameButton;
        public static bool NoEndGame { get; internal set; } = false!;
        public static bool Prefix()
        {
            return !(NoEndGame);
        }
    }
}
