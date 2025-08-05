using System.Collections.Generic;

namespace COG.UI.ClientOption;

public class ClientOptionManager
{
    private static readonly ClientOptionManager Manager = new();
    private readonly List<ClientOption> _options = new();

    public static ClientOptionManager GetManager()
    {
        return Manager;
    }

    public void RegisterClientOption(ClientOption option)
    {
        _options.Add(option);
    }

    public void RegisterClientOptions(ClientOption[] options)
    {
        _options.AddRange(options);
    }

    public List<ClientOption> GetOptions()
    {
        return _options;
    }
}