# First Plugin

### Overview

The COG plugin system has evolved to use **Python (IronPython)** for plugin development, replacing the earlier JavaScript implementation. This guide covers the latest plugin development practices and APIs.

### Plugin Architecture

The plugin system consists of several key components:

#### 1. **PythonPluginManager**
- Manages plugin discovery, loading, and lifecycle
- Handles dependency resolution and plugin sorting
- Manages plugin extraction from `.ca` package files
- Responsible for calling plugin lifecycle callbacks

**Key Methods:**
```csharp
void LoadAllPlugins()        // Discover and load all plugins from plugins folder
void LoadPlugin(string path) // Load a single plugin from a specific path
void UnloadPlugin(Plugin plugin) // Unload a specific plugin
Plugin[] GetPlugins()        // Get all loaded plugins
Plugin? GetPlugin(string name) // Get a plugin by name
void DisableAllPlugins()     // Unload all plugins
```

#### 2. **PythonPluginHandler**
- Manages Python script execution using IronPython
- Creates and manages plugin instances
- Calls lifecycle callbacks (`on_initialize`, `on_shutdown`)
- Injects API and utilities into plugin scope

**Key Methods:**
```csharp
void LoadMainScript()   // Load and instantiate the plugin
void OnInitialize()     // Call plugin's on_initialize() method
void OnShutdown()       // Call plugin's on_shutdown() method
dynamic? GetPythonInstance() // Get the plugin instance
```

#### 3. **PluginDescription**
Represents plugin metadata from `plugin.yml`:
```csharp
public record PluginDescription(
    string Name,              // Plugin name
    string Version,           // Plugin version
    string? Description,      // Plugin description
    string ApiVersion,        // Required COG API version
    string Main,              // Main class reference (e.g., "main.MyPlugin")
    string[]? Authors,        // Plugin authors
    string[]? Depends,        // Hard dependencies
    string[]? SoftDepends,    // Soft dependencies
    string? Website,          // Plugin website/repo
    string Prefix,            // Logging prefix
    string[]? LoadBefore)     // Plugins that should load after this one
```

### Plugin Package Format (.ca files)

COG plugins are distributed as `.ca` files, which are standard ZIP archives containing:

```
plugin.ca
├── plugin.yml          # Plugin metadata (REQUIRED)
├── scripts/            # Python scripts folder (REQUIRED)
│   ├── main.py        # Main plugin file
│   ├── utils.py       # Helper modules
│   └── modules/       # External dependencies
│       └── module.zip
├── resources/         # Resource files (optional)
│   ├── icon.png
│   └── data.json
└── docs/             # Documentation (optional)
```

### plugin.yml Specification

#### Required Fields

```yaml
name: PluginName
version: 1.0.0
api-version: 1.0.0
main: main.PluginClass
```

- **name** (string): Unique plugin identifier
  - Must be alphanumeric with underscores
  - Used for dependency references and logging
  
- **version** (string): Semantic version (e.g., "1.0.0", "2.1.3-beta")
  - Used for version checking
  
- **api-version** (string): COG API version requirement
  - Compared against mod version for compatibility
  - If plugin requires newer API, loading is rejected
  
- **main** (string): Entry point in format `module.ClassName`
  - Example: `main.MyAwesomePlugin`
  - The system will import the module and instantiate the class

#### Optional Fields

```yaml
description: Plugin description here
authors:
  - Author1
  - Author2
website: https://github.com/author/plugin
prefix: PLUG  # Logging prefix (defaults to plugin name)

# Dependency Management
depend:
  - RequiredPlugin1
  - RequiredPlugin2
soft-depend:
  - OptionalPlugin1

# Load Order
load-before:
  - PluginThatLoadsAfterThis
```

- **description** (string): Short description of what the plugin does
- **authors** (list): List of plugin authors
- **website** (string): URL to plugin repository or website
- **prefix** (string): Prefix for plugin logs (defaults to plugin name)
- **depend** (list): Hard dependencies - plugin won't load if these are missing
- **soft-depend** (list): Soft dependencies - plugin loads even if these are missing
- **load-before** (list): Plugins that should load after this one

### Python Plugin Development

#### Basic Plugin Structure

```python
class MyAwesomePlugin:
    """Your plugin class"""
    
    def __init__(self):
        """Constructor - called when plugin instance is created"""
        self.name = "MyAwesomePlugin"
        self.is_enabled = False
        print(f"Plugin {self.name} constructed")
    
    def on_initialize(self):
        """Called when plugin is initialized"""
        print(f"Plugin {self.name} initialized")
        self.is_enabled = True
        # Initialize your plugin here
    
    def on_shutdown(self):
        """Called when plugin is shutting down"""
        print(f"Plugin {self.name} shutting down")
        self.is_enabled = False
        # Cleanup here
```

#### Plugin Lifecycle

1. **Construction**: Plugin class is instantiated with `PluginClass()`
2. **Initialization**: `on_initialize()` is called
3. **Runtime**: Plugin runs and handles events/logic
4. **Shutdown**: `on_shutdown()` is called when unloading

#### Built-in APIs

The plugin system provides access to various built-in functions and objects:

```python
# Logging
logger.info("Message")
logger.warning("Warning")
logger.error("Error")

# Game utilities
game.get_players()
game.current_round
# ... more APIs (see Builtins.md)
```

### Dependency Management

#### Hard Dependencies

If your plugin requires other plugins to function, list them in `depend`:

```yaml
depend:
  - BasePlugin
  - UtilityLibrary
```

Without these plugins, your plugin will **not load**.

#### Soft Dependencies

For optional plugins that enhance functionality but aren't required:

```yaml
soft-depend:
  - EnhancementPlugin
  - OptionalExtension
```

Your plugin will still load even if these are missing. Handle the absence gracefully:

```python
def on_initialize(self):
    # Check if optional plugin is available
    if self.is_plugin_loaded("EnhancementPlugin"):
        self.use_enhancement_plugin()
    else:
        print("Enhancement plugin not available, using basic mode")
```

#### Load Order

Control when your plugin loads relative to others:

```yaml
load-before:
  - LateLoadingPlugin
  - AnotherLatePlugin
```

This ensures your plugin initializes before the specified plugins.

### Development Workflow

#### Step 1: Create Plugin Structure

```bash
mkdir MyAwesomePlugin
cd MyAwesomePlugin
mkdir scripts resources
```

#### Step 2: Write plugin.yml

Create `plugin.yml`:
```yaml
name: MyAwesomePlugin
version: 1.0.0
api-version: 1.0.0
main: main.MyAwesomePlugin
description: An awesome plugin
authors:
  - YourName
```

#### Step 3: Write Main Plugin

Create `scripts/main.py`:
```python
class MyAwesomePlugin:
    def __init__(self):
        self.enabled = False
    
    def on_initialize(self):
        print("MyAwesomePlugin initialized!")
        self.enabled = True
    
    def on_shutdown(self):
        print("MyAwesomePlugin shutting down!")
        self.enabled = False
```

#### Step 4: Package Plugin

```bash
# Compress everything (excluding .git, venv, etc.)
zip -r MyAwesomePlugin.zip MyAwesomePlugin/

# Rename to .ca
mv MyAwesomePlugin.zip MyAwesomePlugin.ca
```

#### Step 5: Install and Test

```bash
# Copy to COG plugins directory
cp MyAwesomePlugin.ca "Path/to/Among Us/ClashOfGods_DATA/plugins/"

# Launch game and check logs
```

### Error Handling

The plugin system provides error messages for common issues:

| Error | Cause | Solution |
|-------|-------|----------|
| "plugin.yml missing" | Missing metadata file | Add `plugin.yml` to plugin root |
| "Main must be in format 'Module.ClassName'" | Invalid main reference | Fix format, e.g., `main.MyClass` |
| "Failed to load python script" | Python syntax error | Check Python code for errors |
| "Missing dependency 'X'" | Required plugin not found | Install required plugin |
| "Plugin X is invented for a higher version" | API version incompatible | Update COG or downgrade plugin |

### Best Practices

1. **Error Handling**: Always wrap operations in try-except blocks
2. **Logging**: Use logger for debugging and info messages
3. **Resource Cleanup**: Always clean up in `on_shutdown()`
4. **Version Management**: Use semantic versioning
5. **Documentation**: Include README and inline code comments
6. **Testing**: Test with dependency plugins present and absent

## Chapter End
See also
[Built-in Functions and Variables](Builtins-English.md):