using System;
using System.Collections.Generic;
using COG.Utils;

namespace COG.Config.Impl;

public class LanguageConfig : ConfigBase
{
    private static readonly Dictionary<string, string> CustomTranslations = new();

#nullable disable
    static LanguageConfig()
    {
        LoadLanguageConfig();
    }
#nullable restore

    private LanguageConfig() : base(
        "Language",
        BasePath + "/language.yml",
        new ResourceFile("COG.Resources.Configs.language.yml"),
        replace: true
    )
    {
    }

    private LanguageConfig(string path) : base("Language", path)
    {
        try
        {
        }
        catch
        {
            GameUtils.Popup?.Show("An error occurred when loading language from the disk.");
            Instance = new LanguageConfig();
        }
    }

    public static LanguageConfig Instance { get; private set; }

    public string GetString(string location)
    {
        if (CustomTranslations.TryGetValue(location, out var custom))
            return custom;

        var toReturn = YamlReader!.GetString(location);
        if (string.IsNullOrWhiteSpace(toReturn))
        {
            Main.Logger.LogDebug($"Missing string: {location}");
            toReturn = location;
        }

        return toReturn;
    }

    public TextHandler GetHandler(string location) => new(location);

    private static void LoadLanguageConfig() => Instance = new();
    internal static void LoadLanguageConfig(string path) => Instance = new(path);

    public static void AddCustomTranslation(string langId, Dictionary<string, string> entries)
    {
        foreach (var (key, value) in entries)
        {
            CustomTranslations[key] = value;
        }
    }

    public static void RemoveCustomTranslation(string langId, IEnumerable<string> keys)
    {
        foreach (var key in keys)
        {
            CustomTranslations.Remove(key);
        }
    }

    public class TextHandler
    {
        internal TextHandler(string location) => Location = location;

        public string Location { get; }

        public string GetString(string target) => Instance.GetString($"{Location}.{target}");
    }
}
