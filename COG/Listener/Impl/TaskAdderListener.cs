using COG.Listener.Event.Impl.TAButton;
using COG.Listener.Event.Impl.TAGame;
using COG.Role;
using COG.Role.Impl;
using COG.Utils;
using System.Linq;

namespace COG.Listener.Impl;

public class TaskAdderListener : IListener
{
    public static TaskFolder? RoleFolder;
    public const string ModdedTaskName = "ModdedRole";

    [EventHandler(EventHandlerType.Prefix)]
    public void OnTaskAdderShowFolder(TaskAdderGameShowFolderEvent @event)
    {
        var taskAdderGame = @event.TaskAdderGame;
        var folder = @event.GetTaskFolder();
        if (taskAdderGame.Root == folder && RoleFolder == null)
        {
            RoleFolder = Object.Instantiate(taskAdderGame.RootFolderPrefab, taskAdderGame.transform);
            RoleFolder.gameObject.SetActive(false);
            RoleFolder.FolderName = Main.DisplayName;

            taskAdderGame.Root.SubFolders.Add(RoleFolder);
        }
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void AfterTaskAdderShowFolder(TaskAdderGameShowFolderEvent @event)
    {
        var taskAdderGame = @event.TaskAdderGame;
        var folder = @event.GetTaskFolder();
        if (RoleFolder != null && RoleFolder.FolderName == folder.FolderName)
        {
            var xCursor = 0f;
            var yCursor = 0f;
            var maxHeight = 0f;

            foreach (var role in CustomRoleManager.GetManager().GetRoles())
            {
                if (role is Unknown) continue;
                var button = Object.Instantiate(taskAdderGame.RoleButton);
                button.Text.text = role.Name;
                taskAdderGame.AddFileAsChild(RoleFolder, button, ref xCursor, ref yCursor, ref maxHeight);

                button.Role = role.VanillaRole;

                button.FileImage.color = button.RolloverHandler.OutColor = role.Color;
                button.RolloverHandler.OverColor = Palette.AcceptedGreen;

                button.name = ModdedTaskName;
            }
        }
    }

    [EventHandler(EventHandlerType.Prefix)]
    public void OnTaskButtonUpdate(TaskAddButtonUpdateEvent @event)
    {
        var button = @event.TaskAddButton;
        if (button.Text.text.StartsWith("Be_"))
        {
            button.gameObject.SetActive(false);
            return;
        }
        var role = button.Role;
        if (button.name != ModdedTaskName) return;
        var customRole = CustomRoleManager.GetManager().GetRoles().FirstOrDefault(c => c.VanillaRole == role);
        if (customRole == null) return;
        button.Overlay.gameObject.SetActive(PlayerControl.LocalPlayer.IsRole(customRole));
    }

    [EventHandler(EventHandlerType.Prefix)]
    public bool OnTaskButtonAddTask(TaskAddButtonAddTaskEvent @event)
    {
        var button = @event.TaskAddButton;
        var role = button.Role;
        if (button.Text.text.StartsWith("Be_")) return true;
        if (button.name != ModdedTaskName) return true;
        var customRole = CustomRoleManager.GetManager().GetRoles().FirstOrDefault(c => c.VanillaRole == role);
        if (customRole == null) return true;
        PlayerControl.LocalPlayer.SetCustomRole(customRole);
        return false;
    }
}