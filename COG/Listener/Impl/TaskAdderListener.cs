using AmongUs.GameOptions;
using COG;
using COG.Listener;
using COG.Role.Impl.Crewmate;
using COG.Role.Impl.Impostor;
using COG.Role.Impl;
using COG.Role;
using COG.Utils;

public class TaskAdderListener : IListener
{
    public static TaskFolder? RoleFolder;
    public static TaskAddButton? LastClicked = null;
    public void OnTaskAdderShowFolder(TaskAdderGame taskAdderGame, TaskFolder folder)
    {
        if (taskAdderGame.Root == folder && RoleFolder == null)
        {
            RoleFolder = UnityEngine.Object.Instantiate(taskAdderGame.RootFolderPrefab, taskAdderGame.transform);
            RoleFolder.gameObject.SetActive(false);
            RoleFolder.FolderName = Main.DisplayName;

            taskAdderGame.Root.SubFolders.Add(RoleFolder);
        }
    }

    public void AfterTaskAdderShowFolder(TaskAdderGame taskAdderGame, TaskFolder folder)
    {
        if (RoleFolder != null && RoleFolder.FolderName == folder.FolderName)
        {
            float xCursor = 0f;
            float yCursor = 0f;
            float maxHeight = 0f;

            foreach (var role in COG.Role.RoleManager.GetManager().GetRoles())
            {
                if (role is Unknown or Crewmate or Impostor) continue;
                var button = UnityEngine.Object.Instantiate(taskAdderGame.RoleButton);
                button.Text.text = role.Name;
                taskAdderGame.AddFileAsChild(RoleFolder, button, ref xCursor, ref yCursor, ref maxHeight);

                RoleBehaviour roleBehaviour = new()
                {
                    Role = (RoleTypes)COG.Role.RoleManager.GetManager().GetRoles().IndexOf(role) + 100
                };
                button.Role = roleBehaviour;

                button.FileImage.color = button.RolloverHandler.OutColor = role.Color;
                button.RolloverHandler.OverColor = Palette.AcceptedGreen;
            }
        }
    }

    public void OnTaskButtonUpdate(TaskAddButton button)
    {
        try
        {
            var role = button.Role;
            int type = 99;
            if (!role && !((type = (ushort)role.Role) > 100)) return;
            if (type is not <= 7 and not 99) button.Overlay.gameObject.SetActive(LastClicked?.Role.Role == button.Role.Role);
        }
        catch
        {
            // Ignored (There isn't any problems with this)
        }
    }

    public bool OnTaskButtonAddTask(TaskAddButton button)
    {
        var role = button.Role;
        int type = 99;
        if (!role && !((type = (ushort)role.Role) > 100)) return true;
        if (type is not <= 7 and not 99)
        {
            PlayerControl.LocalPlayer.SetCustomRole(COG.Role.RoleManager.GetManager().GetRoles()[type - 100]);
            LastClicked = button;
        }
        return false;
    }
}