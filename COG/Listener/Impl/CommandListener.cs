using System;
using System.Linq;
using COG.Command;
using COG.Listener.Event.Impl.Player;

namespace COG.Listener.Impl;

public class CommandListener : IListener
{
    [EventHandler(EventHandlerType.Prefix)]
    public bool OnLocalPlayerChat(LocalPlayerChatEvent @event)
    {
        var text = @event.Text.ToLower();
        if (!text.StartsWith("/")) return true;

        var enteredName = text.Split(" ").FirstOrDefault();
        if (enteredName == null) return true;

        var result = CommandManager.GetManager().GetCommands()
            .FirstOrDefault(c => "/" + c.Name.ToLower() == enteredName || ContainAliases(text, c));
        if (result == null) return true;
        if (result.HostOnly && !AmongUsClient.Instance.AmHost)
            return false;

        return !result.OnExecute(PlayerControl.LocalPlayer, AsCommandStringArray(text));
    }

    // Handling other players' chat command and executing aren't necessary

    //[EventHandler(EventHandlerType.Postfix)]
    //public void OnPlayerChat(PlayerChatEvent @event)
    //{
    //    var text = @event.Text;
    //    var player = @event.Player;
    //    if (!text.ToLower().StartsWith("/")) return;

    //    foreach (var command in CommandManager.GetManager().GetCommands().Where(command => !command.HostOnly)
    //                 .Where(command => text.Split(" ")[0].ToLower().Equals("/" + command.Name.ToLower()) ||
    //                                   ContainAliases(text, command)))
    //        command.OnExecute(player, AsCommandStringArray(text));
    //}

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