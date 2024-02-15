using AmongUs.GameOptions;
using COG.Listener;
using COG.Role;
using COG.Role.Impl;
using COG.Role.Impl.Crewmate;
using COG.Role.Impl.Impostor;
using COG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace COG.Patch
{
    [HarmonyPatch(typeof(TaskAdderGame), nameof(TaskAdderGame.ShowFolder))]
    public static class TaskAdderPatch
    {
        static TaskFolder RoleFolder;
        public static void Prefix(TaskAdderGame __instance, [HarmonyArgument(0)] TaskFolder taskFolder)
        {
            foreach (var listener in ListenerManager.GetManager().GetListeners())
                listener.OnTaskAdderShowFolder(__instance, taskFolder);
        }

        public static void Postfix(TaskAdderGame __instance, [HarmonyArgument(0)] TaskFolder taskFolder)
        {
            foreach (var listener in ListenerManager.GetManager().GetListeners())
                listener.AfterTaskAdderShowFolder(__instance, taskFolder);
        }
    }

    [HarmonyPatch(typeof(TaskAddButton))]
    public static class TaskAddButtonPatch
    {
        static TaskAddButton LastClicked = null;

        [HarmonyPatch(nameof(TaskAddButton.Update))]
        [HarmonyPrefix]
        public static void ButtonUpdatePatch(TaskAddButton __instance)
        {
            foreach (var listener in ListenerManager.GetManager().GetListeners())
                listener.OnTaskButtonUpdate(__instance);
        }

        [HarmonyPatch(nameof(TaskAddButton.AddTask))]
        [HarmonyPrefix]
        public static bool AddTaskPatch(TaskAddButton __instance)
        {
            var returnAble = false;
            foreach (var unused in ListenerManager.GetManager().GetListeners()
                         .Where(listener => !listener.OnTaskButtonAddTask(__instance) && !returnAble)) returnAble = true;

            return !returnAble;
        }
    }
}
