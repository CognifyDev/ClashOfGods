using UnityEngine;

namespace COG.Patch
{
    [HarmonyPatch(typeof(ControllerManager), nameof(ControllerManager.Update))]
    internal class ControllerManagerUpdatePatch 
    {
        public static void Postfix()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                CheckEndCriteriaPatch.NoEndGame = true;
            }
        }
    }

}
