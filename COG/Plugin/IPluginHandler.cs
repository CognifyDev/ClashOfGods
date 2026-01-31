namespace COG.Plugin;

public interface IPluginHandler
{
    void OnInitialize();

    void OnShutdown();
}