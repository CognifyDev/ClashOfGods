using System;
using COG.Command;

namespace COG.Listener.Impl;

public class CommandListener : IListener
{
    public bool OnHostChat(ChatController controller)
    {
        var text = controller.TextArea.text;
        if (!text.ToLower().StartsWith("/")) return true;
        
        bool cancellable = false;
        foreach (var command in CommandManager.GetManager().GetCommands())
        {
            if (text.Split(" ")[0].ToLower().Equals("/" + command.Name.ToLower()) || ContainAliases(text, command))
                command.OnExecute(PlayerControl.LocalPlayer, AsCommandStringArray(text));
            if (command.Cancellable) cancellable = true;
        }

        return !cancellable;
    }

    public bool OnPlayerChat(PlayerControl player, string text)
    {
        if (!text.ToLower().StartsWith("/")) return true;

        bool cancellable = false;
        foreach (var command in CommandManager.GetManager().GetCommands())
        {
            if (command.HostOnly)
            {
                continue;
            }
            
            if (text.Split(" ")[0].ToLower().Equals("/" + command.Name.ToLower()) || ContainAliases(text, command))
                command.OnExecute(player, AsCommandStringArray(text));
            if (command.Cancellable) cancellable = true;
        }

        return !cancellable;
    }
    
    private string[] AsCommandStringArray(string text)
    {
        // /command <arg1> <arg3> <arg2>...
        var args = text.Split(' ');
        var toReturn = new string[args.Length - 1];
        Array.Copy(args, 1, toReturn, 0, toReturn.Length);
        return toReturn;
    }

    private bool ContainAliases(string text, Command.Command command)
    { 
        foreach (var alias in command.Aliases)
        {
            if (text.Split(" ")[0].ToLower().Equals("/" + alias.ToLower())) 
                return true;
        }
        return false;
    }
}