using System.Collections.Generic;

namespace COG.UI.ClientOption;

public class ClientOptionManager
{
    private static readonly ClientOptionManager Manager = new();
    private readonly List<IClientOption> _options = [];

    public static ClientOptionManager GetManager()
    {
        return Manager;
    }

    public void RegisterClientOption(IClientOption option)
    {
        _options.Add(option);
    }

    public void RegisterClientOptions(IEnumerable<IClientOption> options)
    {
        _options.AddRange(options);
    }

    public List<IClientOption> GetOptions()
    {
        return _options;
    }
}