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

    private string[] AsCommandStringArray(string text)
    {
        // /command <arg1> <arg3> <arg2>...
        var args = ;
        return text.Split(' ').Skip(1).ToArray();
    }

    private bool ContainAliases(string text, CommandBase command)
    {
        foreach (var alias in command.Aliases)
            if (text.Split(" ")[0].ToLower().Equals("/" + alias.ToLower()))
                return true;
        return false;
    }
}