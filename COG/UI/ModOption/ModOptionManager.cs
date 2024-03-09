using System.Collections.Generic;

namespace COG.UI.ModOption;

public class ModOptionManager
{
    private static readonly ModOptionManager Manager = new();
    private readonly List<ModOption> _options = new();

    public static ModOptionManager GetManager()
    {
        return Manager;
    }

    public void RegisterModOption(ModOption option)
    {
        _options.Add(option);
    }

    public void RegisterModOptions(ModOption[] options)
    {
        _options.AddRange(options);
    }

    public List<ModOption> GetOptions()
    {
        return _options;
    }
}