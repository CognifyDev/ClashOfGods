using System;

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

    /// <summary>
    /// Get the name of the plugin
    /// </summary>
    /// <returns>plugin name</returns>
    string GetName();

    /// <summary>
    /// Get the author of the plugin
    /// </summary>
    /// <returns>plugin author</returns>
    string GetAuthor();

    /// <summary>
    /// Get the version of the plugin
    /// </summary>
    /// <returns>plugin version</returns>
    string GetVersion();

    /// <summary>
    /// Get the main class of the plugin
    /// </summary>
    /// <returns>plugin main class</returns>
    string GetMainClass();
}