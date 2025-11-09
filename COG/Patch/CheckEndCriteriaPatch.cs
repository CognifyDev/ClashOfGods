using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COG.Constant;
using COG.UI.ClientOption.Impl;
using COG.UI.CustomOption;

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
