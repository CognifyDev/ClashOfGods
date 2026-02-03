using Microsoft.Scripting;

namespace COG.Plugin.Python;

using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System;

public class PythonPluginHandler : IPluginHandler
{
    private readonly ScriptScope _scope;
    private dynamic? _pluginInstance;
    private readonly string _mainClassRef; // e.g., "main.MyPlugin"

    public PythonPluginHandler(ScriptEngine engine, string scriptRootPath, string mainClassRef)
    {
        _mainClassRef = mainClassRef;
        
        _scope = engine.CreateScope();
        
        Inject();

        // !! IMPORTANT !!
        var paths = engine.GetSearchPaths();
        if (!paths.Contains(scriptRootPath))
        {
            paths.Add(scriptRootPath);
            engine.SetSearchPaths(paths);
        }
    }

    private void Inject()
    {
        // TODO: Do some stuff like SetVariable
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
        }
    }
    
    public dynamic? GetPythonInstance() => _pluginInstance;
}