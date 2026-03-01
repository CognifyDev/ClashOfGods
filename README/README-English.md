# ClashOfGods

**An epoch-making Among Us mod with a plugin system that allows you to make your own plugin by EASY computer language Python**

> English | [简体中文](#README-SChinese.md)

### Overview

Clash Of Gods is an advanced Among Us mod that brings innovative gameplay features, custom roles, and an extensible plugin system to the game. Built on the BepInEx framework, it provides both players and developers with a powerful platform for customization and extension.

### Statement

This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC.

### Key Features

- **Custom Roles System** - Add unique roles with special abilities to the game
- **Advanced Game Options** - Extensive customization options for game rules and settings
- **Listener/Event System** - Hook into game events with custom listeners
- **Python Plugin System** - Extend the mod using simple Python programming

### About the Mod

Clash Of Gods adds many interesting custom roles and features to the vanilla game, enabling fresh gameplay experiences and fostering a creative community around mod development.

### Plugin System

We have built a powerful Python-based plugin system for developers. The system has evolved from the original JavaScript implementation to now use **Python (IronPython)** for plugin development, providing greater flexibility and ease of use.

#### Plugin Architecture

- **Plugin Manager**: Manages the lifecycle of plugins (loading, initialization, shutdown)
- **Plugin Handler**: Executes Python scripts and manages plugin instances
- **Dependency System**: Supports both hard and soft dependencies between plugins
- **Package Format**: `.ca` files (ZIP archives with plugin.yml metadata)

#### Key Components

| Component | Purpose |
|-----------|---------|
| `PythonPluginManager` | Manages plugin loading, initialization, and lifecycle |
| `PythonPluginHandler` | Executes Python scripts using IronPython engine |
| `PluginDescription` | Parses and stores plugin metadata from `plugin.yml` |
| `IPluginManager` | Interface for plugin management operations |
| `IPluginHandler` | Interface for plugin lifecycle callbacks |

#### Plugin Lifecycle

1. **Load Phase** - Plugin file is extracted and metadata is parsed
2. **Initialize Phase** - Python main script is loaded and plugin instance is created
3. **Runtime Phase** - Plugin executes `on_initialize()` callback and runs active
4. **Shutdown Phase** - Plugin executes `on_shutdown()` callback when unloaded

### Getting Started with Plugin Development

#### Plugin Structure

Create your plugin with the following directory structure:

```
MyAwesomePlugin/
├── scripts/
│   ├── main.py
│   ├── utils.py
│   └── modules/
│       └── mymodule.zip
├── resources/
│   ├── icon.png
│   └── ...
├── plugin.yml
└── ...
```

#### plugin.yml Configuration

```yaml
name: MyAwesomePlugin
version: 1.0.0
api-version: 1.0.0
main: main.MyAwesomePlugin
description: An awesome plugin for COG
authors:
  - YourName
  - OtherContributor
website: https://github.com/yourname/myawe soplugin

# Dependencies
depend:
  - SomeRequiredPlugin
soft-depend:
  - OptionalPlugin

# Loading order
load-before:
  - LateLoadingPlugin

# Plugin prefix for logging
prefix: MAP
```

**Required Fields:**
- `name` - Plugin identifier
- `version` - Plugin version (semantic versioning)
- `api-version` - COG API version this plugin is built for
- `main` - Entry point in format `module.ClassName`

**Optional Fields:**
- `description` - Plugin description
- `authors` - List of authors
- `website` - Plugin website/repository URL
- `depend` - Hard dependencies (plugin fails to load if missing)
- `soft-depend` - Soft dependencies (plugin loads anyway if missing)
- `load-before` - Plugins that should load after this one
- `prefix` - Custom prefix for plugin logging

#### Basic Python Plugin Example

```python
class MyAwesomePlugin:
    def __init__(self):
        self.enabled = False
    
    def on_initialize(self):
        """Called when plugin is initialized"""
        print("MyAwesomePlugin initialized!")
        self.enabled = True
    
    def on_shutdown(self):
        """Called when plugin is being unloaded"""
        print("MyAwesomePlugin shutting down!")
        self.enabled = False
```

#### Plugin Installation

1. Create your plugin directory structure as shown above
2. Compress the entire plugin directory into a ZIP file
3. Rename the `.zip` file to `.ca` (CArabiner format)
4. Place the `.ca` file in `ClashOfGods_DATA/plugins/` directory
5. Launch the game - your plugin will be automatically discovered and loaded

> **Note:** The `ClashOfGods_DATA` directory is located in your Among Us installation directory (where `Among Us.exe` is located).

### Plugin API Reference

#### Available Callbacks

```python
def on_initialize(self):
    """Called when the plugin is initialized"""
    pass

def on_shutdown(self):
    """Called when the plugin is shutting down"""
    pass
```

#### Built-in Functions and Objects

The plugin system provides access to various built-in functions for logging, game interaction, and more. Refer to the [Built-in Functions Guide](README/Plugin/Builtins-English.md) for comprehensive API documentation.

### Dependency Management

COG supports both hard and soft dependencies for plugins:

- **Hard Dependencies** (`depend`): Plugins listed here must be present for your plugin to load
- **Soft Dependencies** (`soft-depend`): Plugins listed here don't need to be present, but your plugin should handle their absence gracefully
- **Load Before** (`load-before`): Specifies plugins that should load after your plugin

The plugin system automatically sorts plugins based on dependencies before loading.

### Development

The MOD is still under active development. If you're skilled in Among Us modding or Python development, we welcome your contributions!

### Resources

- **Plugin Development Guide**: [First Plugin Guide](README/Plugin/FirstPlugin-English.md)
- **Built-in API Reference**: [Built-in Functions](README/Plugin/Builtins-English.md)
- **Plugin System Overview**: [About Plugin System](README/Plugin/About-English.md)

### Community

Join our community to discuss, share, and collaborate:
- **Discord**: [Join our Discord server](https://discord.gg/uWZGh4Chde)