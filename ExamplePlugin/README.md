# Example Plugin - Medic Role

Demonstrates COG's plugin system with C# Roslyn compilation, multi-language support, and custom resources.

## Structure
```
ExamplePlugin.zip
├── metainfo.json          # Required metadata
├── Scripts/
│   └── MedicRole.cs       # C# source (auto-compiled at runtime)
├── Languages/
│   ├── English.yaml       # English translations
│   └── Chinese.yaml       # Chinese translations
└── Resources/
    └── shield.png         # Custom button icon (you must add this)
```

## Build
1. Place a `shield.png` (any size) in `Resources/`
2. Run `build.bat` to create `ExamplePlugin.zip`
3. Copy `ExamplePlugin.zip` to `{AmongUs}/ClashOfGods_DATA/Resources/`
4. Launch the game

## How it works
- `metainfo.json` tells COG this is a `Plugin` type resource
- On startup, `ResourcesManager` scans `ClashOfGods_DATA/Resources/` for `.zip` files
- C# source in `Scripts/` is compiled at runtime using .NET SDK's Roslyn compiler
- `[PluginModuleInitializer]` method is called to register the role
- `PluginContext.Current.GetString("key")` reads from the plugin's own `Languages/` files
- `PluginContext.Current.GetSprite("path")` loads images from the plugin's own `Resources/` folder
