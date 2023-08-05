using System.Collections.Generic;

namespace COG.UI.ModOption;

public class ModOptionManager
{
    private static readonly ModOptionManager _manager = new();
    private readonly List<ModOption> Options = new();

    public static ModOptionManager GetManager()
    {
        return _manager;
    }

    public void RegisterModOption(ModOption option)
    {
        Options.Add(option);
    }

    public void RegisterModOptions(ModOption[] options)
    {
        Options.AddRange(options);
    }

    public List<ModOption> GetOptions()
    {
        return Options;
    }
}