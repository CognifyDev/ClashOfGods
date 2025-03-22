using System.Collections.Generic;

namespace COG.Command;

public class CommandManager
{
    private static CommandManager? _manager;

    private readonly List<CommandBase> _commands = new();

    public static CommandManager GetManager()
    {
        return _manager ??= new CommandManager();
    }

    public void RegisterCommand(CommandBase command)
    {
        _commands.Add(command);
    }

    public void RegisterCommands(CommandBase[] commands)
    {
        _commands.AddRange(commands);
    }

    public List<CommandBase> GetCommands()
    {
        return _commands;
    }
}