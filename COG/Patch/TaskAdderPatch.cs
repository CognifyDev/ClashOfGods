using AmongUs.GameOptions;
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

namespace COG.Patch
{
    [HarmonyPatch(typeof(TaskAdderGame),nameof(TaskAdderGame.ShowFolder))]
    public static class TaskAdderPatch
    {
        static TaskFolder RoleFolder;
        public static void Prefix(TaskAdderGame __instance, [HarmonyArgument(0)] TaskFolder taskFolder)
        {
            if (__instance.Root == taskFolder && RoleFolder == null)
            {
                RoleFolder = UnityEngine.Object.Instantiate(__instance.RootFolderPrefab, __instance.transform);
                RoleFolder.gameObject.SetActive(false);
                RoleFolder.FolderName = Main.DisplayName;

                __instance.Root.SubFolders.Add(RoleFolder);
            }
        }

        public static void Postfix(TaskAdderGame __instance, [HarmonyArgument(0)] TaskFolder taskFolder)
        {
            if (RoleFolder != null && RoleFolder.FolderName == taskFolder.FolderName)
            {
                float xCursor = 0f;
                float yCursor = 0f;
                float maxHeight = 0f;

                foreach (var role in Role.RoleManager.GetManager().GetRoles())
                {
                    if (role is Unknown or Crewmate or Impostor) continue;
                    var button = UnityEngine.Object.Instantiate(__instance.RoleButton);
                    button.Text.text = role.Name;
                    __instance.AddFileAsChild(RoleFolder, button, ref xCursor, ref yCursor, ref maxHeight);

                    RoleBehaviour roleBehaviour = new()
                    {
                        Role = (RoleTypes)Role.RoleManager.GetManager().GetRoles().IndexOf(role) + 100
                    };
                    button.Role = roleBehaviour;
                    
                    button.FileImage.color = button.RolloverHandler.OutColor = role.Color;
                    button.RolloverHandler.OverColor = Palette.AcceptedGreen;
                }
            }
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
            var role = __instance.Role;
            int type = 99;
            if (!role && !((type = (ushort)role.Role) > 100)) return;
            if (type is not <= 7 and not 99) __instance.Overlay.gameObject.SetActive(LastClicked.Role.Role==__instance.Role.Role/*PlayerControl.LocalPlayer.IsRole(Role.RoleManager.GetManager().GetRoles()[type - 100]*/);
        }

        [HarmonyPatch(nameof(TaskAddButton.AddTask))]
        [HarmonyPrefix]
        public static bool AddTaskPatch(TaskAddButton __instance)
        {
            var role = __instance.Role;
            int type = 99;
            if (!role && !((type = (ushort)role.Role) > 100)) return true;
            if (type is not <= 7 and not 99)
            {
                PlayerControl.LocalPlayer.SetCustomRole(Role.RoleManager.GetManager().GetRoles()[type - 100]);
                LastClicked = __instance;
            }
            return false;
        }
    }
}
