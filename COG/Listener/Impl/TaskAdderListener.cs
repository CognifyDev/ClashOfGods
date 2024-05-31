using AmongUs.GameOptions;
using COG.Listener.Event.Impl.TAButton;
using COG.Listener.Event.Impl.TAGame;
using COG.Role;
using COG.Role.Impl;
using COG.Role.Impl.Crewmate;
using COG.Role.Impl.Impostor;
using COG.Utils;

namespace COG.Listener.Impl;

public class TaskAdderListener : IListener
{
    public static TaskFolder? RoleFolder;
    public static TaskAddButton? LastClicked;

    [EventHandler(EventHandlerType.Prefix)]
    public void OnTaskAdderShowFolder(TaskAdderGameShowFolderEvent @event)
    {
        var taskAdderGame = @event.TaskAdderGame;
        var folder = @event.GetTaskFolder();
        if (taskAdderGame.Root == folder && RoleFolder == null)
        {
            RoleFolder = UnityEngine.Object.Instantiate(taskAdderGame.RootFolderPrefab, taskAdderGame.transform);
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
                if (role is Unknown or Crewmate or Impostor) continue;
                var button = UnityEngine.Object.Instantiate(taskAdderGame.RoleButton);
                button.Text.text = role.Name;
                taskAdderGame.AddFileAsChild(RoleFolder, button, ref xCursor, ref yCursor, ref maxHeight);

                RoleBehaviour roleBehaviour = new()
                {
                    Role = (RoleTypes)CustomRoleManager.GetManager().GetRoles().IndexOf(role) + 100
                };
                button.Role = roleBehaviour;

                button.FileImage.color = button.RolloverHandler.OutColor = role.Color;
                button.RolloverHandler.OverColor = Palette.AcceptedGreen;
            }
        }
    }

    [EventHandler(EventHandlerType.Prefix)]
    public void OnTaskButtonUpdate(TaskAddButtonUpdateEvent @event)
    {
        var button = @event.TaskAddButton;
        try
        {
            if (button.Text.text.StartsWith("Be_")) return;
            var role = button.Role;
            var type = 99;
            if (!role && !((type = (ushort)role.Role) > 100)) return;
            if (type is > 7 and not 99)
                button.Overlay.gameObject.SetActive(LastClicked!.Role.Role == button.Role.Role);
        }
        catch
        {
            // Ignored (There isn't any problems with this)
        }
    }

    [EventHandler(EventHandlerType.Prefix)]
    public bool OnTaskButtonAddTask(TaskAddButtonAddTaskEvent @event)
    {
        var button = @event.TaskAddButton;
        var role = button.Role;
        var type = 99;
        if (button.Text.text.StartsWith("Be_")) return true;
        if (!role && !((type = (ushort)role.Role) > 100)) return true;
        if (type is > 7 and not 99)
        {
            PlayerControl.LocalPlayer.SetCustomRole(CustomRoleManager.GetManager().GetRoles()[type - 100]);
            LastClicked = button;
        }

        return false;
    }
}