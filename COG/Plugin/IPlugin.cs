namespace COG.Plugin;

public interface IPlugin
{
    /// <summary>
    /// This function will be executed when this plugin loaded
    /// </summary>
    void OnEnable();

    /// <summary>
    /// This function will be executed when this plugin disabled
    /// </summary>
    void OnDisable();
}