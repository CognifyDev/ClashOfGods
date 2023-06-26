using System.Collections.Generic;

namespace COG.Command;

public class CommandManager
{
    private static CommandManager? _manager;
    
    private readonly List<Command> _commands = new();

    public static CommandManager GetManager()
    {
        return _manager ??= new();
    }

    public void RegisterCommand(Command command)
    {
        _commands.Add(command);
    }
    
    public void RegisterCommands(Command[] commands)
    {
        _commands.AddRange(commands);
    }

    public List<Command> GetCommands() => _commands;

}