using System;
using System.Linq;
using COG.Command;
using COG.NewListener.Event.Impl.Player;

namespace COG.NewListener.Impl;

public class CommandListener : IListener
{
    [EventHandler(EventHandlerType.Prefix)]
    public bool OnLocalPlayerChat(LocalPlayerChatEvent @event)
    {
        var text = @event.GetChatController().freeChatField.textArea.text;
        if (!text.ToLower().StartsWith("/")) return true;

        var cancellable = false;
        foreach (var command in CommandManager.GetManager().GetCommands())
        {
            if (text.Split(" ")[0].ToLower().Equals("/" + command.Name.ToLower()) || ContainAliases(text, command))
                command.OnExecute(PlayerControl.LocalPlayer, AsCommandStringArray(text));
            if (command.Cancellable) cancellable = true;
        }

        return !cancellable;
    }
    
    [EventHandler(EventHandlerType.Postfix)]
    public void OnPlayerChat(PlayerChatEvent @event)
    {
        var text = @event.Text;
        var player = @event.Player;
        if (!text.ToLower().StartsWith("/")) return;
        
        foreach (var command in CommandManager.GetManager().GetCommands().Where(command => !command.HostOnly).Where(command => text.Split(" ")[0].ToLower().Equals("/" + command.Name.ToLower()) || ContainAliases(text, command)))
        {
            command.OnExecute(player, AsCommandStringArray(text));
        }
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
            if (text.Split(" ")[0].ToLower().Equals("/" + alias.ToLower()))
                return true;
        return false;
    }
}