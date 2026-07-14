using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx;
using COG.Utils;
using UnityEngine;

namespace COG.Config;

public class ConfigBase
{
    public const string DataDirectoryName = $"{Main.DisplayName}_DATA";

    public static string BasePath { get; } = OperatingSystem.IsAndroid()
        ? System.IO.Path.Combine(Application.persistentDataPath, DataDirectoryName)
        : System.IO.Path.Combine(Paths.GameRootPath, DataDirectoryName);

    public ConfigBase(string name, string path, string text, bool replace = false)
    {
        Name = name;
        Path = path;
        Text = text;
        Configs.Add(this);

        LoadConfigs(replace);
    }

    protected ConfigBase(string name, string path, ResourceFile resourceFile, bool replace = false)
    {
        Name = name;
        Path = path;
        Text = resourceFile.GetResourcesText();
        Configs.Add(this);

        LoadConfigs(replace);
    }

    public ConfigBase(string name, string path, bool replace = false)
    {
        Name = name;
        Path = path;
        Text = "";
        Configs.Add(this);

        LoadConfigs(replace);
    }

    public static bool AutoReplace { get; set; }

    public static List<ConfigBase> Configs { get; } = [];
    public string Name { get; }
    public string Path { get; }
    public string Text { get; protected set; }
    public Yaml? YamlReader { get; private set; }

    public void LoadConfigs(bool replace = false)
    {
        if (!Directory.Exists(BasePath)) Directory.CreateDirectory(BasePath);

        if (File.Exists(Path) && (replace || AutoReplace))
            File.Copy(Path, Path + $".old.{DateTime.Now:yyyyMMdd_HHmmss}", true);

        if (!File.Exists(Path) || replace || AutoReplace)
            File.WriteAllText(Path, Text, Encoding.UTF8);
        else
            Text = File.ReadAllText(Path, Encoding.UTF8);

        YamlReader = Yaml.LoadFromString(Text);
        AutoReplace = false;
    }
}
