using Microsoft.Scripting;

namespace COG.Plugin.Python;

using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System;

public class PythonPluginHandler : IPluginHandler
{
    private readonly ScriptScope _scope;
    private dynamic? _pluginInstance;
    private readonly string _mainClassRef;

    public PythonPluginHandler(ScriptEngine engine, string scriptRootPath, string mainClassRef, IPluginResourceIO? resourceIO = null)
    {
        _mainClassRef = mainClassRef;

        _scope = engine.CreateScope();

        if (resourceIO != null)
        {
            _scope.SetVariable("resources", resourceIO);
        }

        var paths = engine.GetSearchPaths();
        if (!paths.Contains(scriptRootPath))
        {
            paths.Add(scriptRootPath);
            engine.SetSearchPaths(paths);
        }
    }

    public void LoadMainScript()
    {
        try
        {
            var parts = _mainClassRef.Split('.');
            if (parts.Length != 2)
                throw new ArgumentException("Main must be in format 'Module.ClassName'");

            var moduleName = parts[0];
            var className = parts[1];

            _scope.ImportModule(moduleName);

            var module = _scope.GetVariable(moduleName);
            var classType = module.GetAttr(className);
            _pluginInstance = classType();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load python script {_mainClassRef}: {ex.Message}", ex);
        }
    }

    public void OnInitialize()
    {
        if (_pluginInstance == null) return;

        try
        {
            _pluginInstance.on_initialize();
        }
        catch (ArgumentTypeException)
        {
            Main.Logger.LogDebug($"[PythonPlugin] on_initialize() not defined or has wrong signature in '{_mainClassRef}'.");
        }
        catch (Exception ex)
        {
            Main.Logger.LogError($"[PythonPlugin] Error during on_initialize() in '{_mainClassRef}': {ex.Message}");
        }
    }

    public void OnShutdown()
    {
        if (_pluginInstance == null) return;

        try
        {
            _pluginInstance.on_shutdown();
        }
        catch (ArgumentTypeException)
        {
            Main.Logger.LogDebug($"[PythonPlugin] on_shutdown() not defined or has wrong signature in '{_mainClassRef}'.");
        }
        catch (Exception ex)
        {
            Main.Logger.LogError($"[PythonPlugin] Error during on_shutdown() in '{_mainClassRef}': {ex.Message}");
        }
    }

    public dynamic? GetPythonInstance() => _pluginInstance;
}
