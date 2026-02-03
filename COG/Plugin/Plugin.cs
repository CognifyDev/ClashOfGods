namespace COG.Plugin;

public record Plugin(PluginDescription PluginDescription, IPluginHandler PluginHandler);